using CommandsProj;
using CommandsProj.CommandExceptions;
using CommandsProj.Commands.ProcessorCommands;
using CommandsProj.States;

namespace Tests
{
    public class HW7CommandProcessorTest
    {
        private sealed class ActionCommand : ICommand
        {
            private readonly Action _action;

            public ActionCommand(Action action)
            {
                _action = action;
            }

            public void Execute()
            {
                _action();
            }
        }

        private sealed class ThrowCommand : ICommand
        {
            public void Execute()
            {
                throw new InvalidOperationException();
            }
        }

        private static void WaitUntilProcessorStopped(CommandProcessorThread processor)
        {
            Assert.That(SpinWait.SpinUntil(() => !processor.IsRunning, TimeSpan.FromSeconds(2)), Is.True);
        }

        [Test]
        public void StartCommandStartsProcessorThread()
        {
            QueueICommand queue = new QueueICommand();
            CommandProcessorThread processor = new CommandProcessorThread(queue);
            using ManualResetEventSlim commandExecuted = new ManualResetEventSlim(false);

            queue.Enqueue(new ActionCommand(commandExecuted.Set));

            new StartProcessorCommand(processor).Execute();

            Assert.That(commandExecuted.Wait(TimeSpan.FromSeconds(2)), Is.True);
            Assert.That(processor.IsRunning, Is.True);

            new HardStopProcessorCommand(processor).Execute();
            WaitUntilProcessorStopped(processor);
        }

        [Test]
        public void HardStopCommandStopsProcessorThread()
        {
            QueueICommand queue = new QueueICommand();
            CommandProcessorThread processor = new CommandProcessorThread(queue);

            new StartProcessorCommand(processor).Execute();
            Assert.That(SpinWait.SpinUntil(() => processor.IsRunning, TimeSpan.FromSeconds(2)), Is.True);

            new HardStopProcessorCommand(processor).Execute();

            WaitUntilProcessorStopped(processor);
        }

        [Test]
        public void SoftStopCommandStopsProcessorAfterAllQueuedCommandsFinished()
        {
            QueueICommand queue = new QueueICommand();
            CommandProcessorThread processor = new CommandProcessorThread(queue);
            using ManualResetEventSlim firstCommandStarted = new ManualResetEventSlim(false);
            using ManualResetEventSlim releaseFirstCommand = new ManualResetEventSlim(false);
            using ManualResetEventSlim secondCommandExecuted = new ManualResetEventSlim(false);
            using ManualResetEventSlim thirdCommandExecuted = new ManualResetEventSlim(false);

            queue.Enqueue(new ActionCommand(() =>
            {
                firstCommandStarted.Set();
                releaseFirstCommand.Wait(TimeSpan.FromSeconds(5));
            }));
            queue.Enqueue(new ActionCommand(secondCommandExecuted.Set));
            queue.Enqueue(new ActionCommand(thirdCommandExecuted.Set));

            new StartProcessorCommand(processor).Execute();
            Assert.That(firstCommandStarted.Wait(TimeSpan.FromSeconds(2)), Is.True);

            new SoftStopProcessorCommand(processor).Execute();

            Assert.That(processor.IsRunning, Is.True);
            Assert.That(queue.isAddingStopped, Is.True);
            Assert.Throws<QueueAddCommandException>(() => queue.Enqueue(new ActionCommand(() => { })));
            Assert.That(secondCommandExecuted.IsSet, Is.False);
            Assert.That(thirdCommandExecuted.IsSet, Is.False);

            releaseFirstCommand.Set();

            Assert.That(secondCommandExecuted.Wait(TimeSpan.FromSeconds(2)), Is.True);
            Assert.That(thirdCommandExecuted.Wait(TimeSpan.FromSeconds(2)), Is.True);
            WaitUntilProcessorStopped(processor);
        }

        [Test]
        public void CommandExceptionDoesNotStopProcessorThread()
        {
            QueueICommand queue = new QueueICommand();
            CommandProcessorThread processor = new CommandProcessorThread(queue);
            using ManualResetEventSlim commandAfterExceptionExecuted = new ManualResetEventSlim(false);

            queue.Enqueue(new ThrowCommand());
            queue.Enqueue(new ActionCommand(commandAfterExceptionExecuted.Set));

            new StartProcessorCommand(processor).Execute();

            Assert.That(commandAfterExceptionExecuted.Wait(TimeSpan.FromSeconds(2)), Is.True);

            new HardStopProcessorCommand(processor).Execute();
            WaitUntilProcessorStopped(processor);
        }

        [Test]
        public void NormalStateExecutesCommandAndReturnsItself()
        {
            QueueICommand queue = new QueueICommand();
            bool executed = false;
            NormalState state = new NormalState(queue);
            queue.Enqueue(new ActionCommand(() => executed = true));

            ICommandProcessorState? nextState = state.Handle(CancellationToken.None);

            Assert.That(executed, Is.True);
            Assert.That(nextState, Is.SameAs(state));
        }

        [Test]
        public void MoveToCommandChangesModeStartingWithNextCommand()
        {
            QueueICommand sourceQueue = new QueueICommand();
            QueueICommand targetQueue = new QueueICommand();
            bool executed = false;
            NormalState normalState = new NormalState(sourceQueue);
            ICommand commandToMove = new ActionCommand(() => executed = true);
            sourceQueue.Enqueue(new MoveToCommand(targetQueue));
            sourceQueue.Enqueue(commandToMove);

            ICommandProcessorState? moveToState = normalState.Handle(CancellationToken.None);
            ICommandProcessorState? nextState = moveToState!.Handle(CancellationToken.None);

            Assert.That(moveToState, Is.TypeOf<MoveToState>());
            Assert.That(nextState, Is.SameAs(moveToState));
            Assert.That(executed, Is.False);
            Assert.That(targetQueue.TryDequeue(out ICommand? movedCommand), Is.True);
            Assert.That(movedCommand, Is.SameAs(commandToMove));
        }

        [Test]
        public void RunCommandReturnsMoveToStateToNormalMode()
        {
            QueueICommand sourceQueue = new QueueICommand();
            QueueICommand targetQueue = new QueueICommand();
            MoveToState state = new MoveToState(sourceQueue, targetQueue);
            sourceQueue.Enqueue(new RunCommand());

            ICommandProcessorState? nextState = state.Handle(CancellationToken.None);

            Assert.That(nextState, Is.TypeOf<NormalState>());
            Assert.That(targetQueue.IsEmpty, Is.True);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void HardStopCommandReturnsNullFromAnyState(bool normalMode)
        {
            QueueICommand sourceQueue = new QueueICommand();
            QueueICommand targetQueue = new QueueICommand();
            ICommandProcessorState state = normalMode
                ? new NormalState(sourceQueue, targetQueue)
                : new MoveToState(sourceQueue, targetQueue);
            sourceQueue.Enqueue(new HardStopCommand());

            ICommandProcessorState? nextState = state.Handle(CancellationToken.None);

            Assert.That(nextState, Is.Null);
        }

        [Test]
        public void ProcessorSwitchesBetweenNormalAndMoveToModes()
        {
            QueueICommand sourceQueue = new QueueICommand();
            QueueICommand targetQueue = new QueueICommand();
            CommandProcessorThread processor = new CommandProcessorThread(sourceQueue);
            using ManualResetEventSlim normalCommandExecuted = new ManualResetEventSlim(false);
            ICommand movedCommand = new ActionCommand(() => { });

            sourceQueue.Enqueue(new MoveToCommand(targetQueue));
            sourceQueue.Enqueue(movedCommand);
            sourceQueue.Enqueue(new RunCommand());
            sourceQueue.Enqueue(new ActionCommand(normalCommandExecuted.Set));
            sourceQueue.Enqueue(new HardStopCommand());

            processor.Start();

            Assert.That(normalCommandExecuted.Wait(TimeSpan.FromSeconds(2)), Is.True);
            WaitUntilProcessorStopped(processor);
            Assert.That(targetQueue.TryDequeue(out ICommand? actualMovedCommand), Is.True);
            Assert.That(actualMovedCommand, Is.SameAs(movedCommand));
        }
    }
}
