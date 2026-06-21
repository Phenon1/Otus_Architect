using CommandsProj.Commands.ProcessorCommands;

namespace CommandsProj.States
{
    public sealed class NormalState : ICommandProcessorState
    {
        private readonly QueueICommand _sourceQueue;
        private readonly QueueICommand? _defaultMoveToQueue;

        public NormalState(QueueICommand sourceQueue, QueueICommand? defaultMoveToQueue = null)
        {
            _sourceQueue = sourceQueue ?? throw new ArgumentNullException(nameof(sourceQueue));
            _defaultMoveToQueue = defaultMoveToQueue;
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
                MoveToCommand moveToCommand => new MoveToState(
                    _sourceQueue,
                    moveToCommand.TargetQueue ?? _defaultMoveToQueue
                        ?? throw new InvalidOperationException(
                            "Для перехода в MoveToState необходимо указать целевую очередь.")),
                _ => ExecuteCommand(command)
            };
        }

        private ICommandProcessorState ExecuteCommand(ICommand command)
        {
            _sourceQueue.ExecuteCommand(command);
            return this;
        }
    }
}
