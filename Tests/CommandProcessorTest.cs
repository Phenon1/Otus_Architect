using CommandsProj;
using CommandsProj.CommandExceptions;
using CommandsProj.Commands.ProcessorCommands;

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
    }
}
