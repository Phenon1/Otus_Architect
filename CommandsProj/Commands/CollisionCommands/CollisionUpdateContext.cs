using ModelsProj;

namespace CommandsProj.Commands.CollisionCommands
{
    public class CollisionUpdateContext
    {
        private readonly CollisionCommandFactory _collisionCommandFactory;
        private readonly HashSet<IUObject> _seenCandidates = new HashSet<IUObject>(UObjectReferenceEqualityComparer.Instance);
        private readonly List<ICommand> _commands = new List<ICommand>();

        public CollisionUpdateContext(IUObject movingObject, CollisionCommandFactory collisionCommandFactory)
        {
            MovingObject = movingObject ?? throw new ArgumentNullException(nameof(movingObject));
            _collisionCommandFactory = collisionCommandFactory ?? throw new ArgumentNullException(nameof(collisionCommandFactory));
        }

        public IUObject MovingObject { get; }

        public IReadOnlyList<ICommand> Commands => _commands.AsReadOnly();

        public bool TryAddCollisionCandidate(IUObject candidate)
        {
            ArgumentNullException.ThrowIfNull(candidate);

            if (ReferenceEquals(MovingObject, candidate) || !_seenCandidates.Add(candidate))
            {
                return false;
            }

            var command = _collisionCommandFactory(MovingObject, candidate)
                ?? throw new InvalidOperationException("Фабрика команд проверки коллизий вернула null.");

            _commands.Add(command);
            return true;
        }

        public List<ICommand> ToCommandList()
        {
            return new List<ICommand>(_commands);
        }

        public void Reset()
        {
            _seenCandidates.Clear();
            _commands.Clear();
        }
    }
}
