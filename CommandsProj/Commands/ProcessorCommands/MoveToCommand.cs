namespace CommandsProj.Commands.ProcessorCommands
{
    public sealed class MoveToCommand : ICommand
    {
        public MoveToCommand()
        {
        }

        public MoveToCommand(QueueICommand targetQueue)
        {
            TargetQueue = targetQueue ?? throw new ArgumentNullException(nameof(targetQueue));
        }

        public QueueICommand? TargetQueue { get; }

        public void Execute()
        {
        }
    }
}
