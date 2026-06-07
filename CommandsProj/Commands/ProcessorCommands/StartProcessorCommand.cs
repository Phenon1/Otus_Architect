namespace CommandsProj.Commands.ProcessorCommands
{
    public class StartProcessorCommand : ICommand
    {
        private readonly CommandProcessorThread _processor;

        public StartProcessorCommand(CommandProcessorThread processor)
        {
            _processor = processor;
        }

        public void Execute()
        {
            _processor.Start();
        }
    }
}
