namespace CommandsProj.States
{
    public interface ICommandProcessorState
    {
        ICommandProcessorState? Handle();

        ICommandProcessorState? Handle(CancellationToken cancellationToken);
    }
}
