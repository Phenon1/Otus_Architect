using ModelsProj;

namespace CommandsProj.Commands.CollisionCommands
{
    public delegate ICommand CollisionCommandFactory(IUObject first, IUObject second);
}
