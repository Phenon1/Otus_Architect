using CommandsProj.Commands.ProcessorCommands;

namespace CommandsProj.States
{
    public sealed class MoveToState : ICommandProcessorState
    {
        private readonly QueueICommand _sourceQueue;
        private readonly QueueICommand _targetQueue;

        public MoveToState(QueueICommand sourceQueue, QueueICommand targetQueue)
        {
            _sourceQueue = sourceQueue ?? throw new ArgumentNullException(nameof(sourceQueue));
            _targetQueue = targetQueue ?? throw new ArgumentNullException(nameof(targetQueue));
        }

        public ICommandProcessorState? Handle()
        {
            return Handle(CancellationToken.None);
        }

        public ICommandProcessorState? Handle(CancellationToken cancellationToken)
        {
            ICommand command = _sourceQueue.Dequeue(cancellationToken);

            return command switch
            {
                HardStopCommand => null,
                RunCommand => new NormalState(_sourceQueue, _targetQueue),
                _ => MoveCommand(command)
            };
        }

        private ICommandProcessorState MoveCommand(ICommand command)
        {
            _targetQueue.Enqueue(command);
            return this;
        }
    }
}
