namespace CommandsProj.Commands.ProcessorCommands
{
    public class SoftStopProcessorCommand : ICommand
    {
        private readonly CommandProcessorThread _processor;

        public SoftStopProcessorCommand(CommandProcessorThread processor)
        {
            _processor = processor;
        }

        public void Execute()
        {
            _processor.SoftStop();
        }
    }
}
