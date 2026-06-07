namespace CommandsProj.Commands.ProcessorCommands
{
    public class HardStopProcessorCommand : ICommand
    {
        private readonly CommandProcessorThread _processor;

        public HardStopProcessorCommand(CommandProcessorThread processor)
        {
            _processor = processor;
        }

        public void Execute()
        {
            _processor.HardStop();
        }
    }
}
