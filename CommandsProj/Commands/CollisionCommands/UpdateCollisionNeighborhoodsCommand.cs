using CommandsProj.Commands;
using ModelsProj;
using ModelsProj.Adapters;
using ModelsProj.Classes;

namespace CommandsProj.Commands.CollisionCommands
{
    public class UpdateCollisionNeighborhoodsCommand : ICommand
    {
        private readonly IReadOnlyList<NeighborhoodSystem> _neighborhoodSystems;
        private readonly IUObject _movingObject;
        private readonly CollisionCommandFactory _collisionCommandFactory;
        private readonly Func<IUObject, Point> _locationProvider;
        private readonly string _macroPropertyName;

        public UpdateCollisionNeighborhoodsCommand(
            IEnumerable<NeighborhoodSystem> neighborhoodSystems,
            IUObject movingObject,
            CollisionCommandFactory collisionCommandFactory,
            Func<IUObject, Point>? locationProvider = null,
            string macroPropertyName = CollisionNeighborhoodConstants.CollisionMacroPropertyName)
        {
            _neighborhoodSystems = (neighborhoodSystems ?? throw new ArgumentNullException(nameof(neighborhoodSystems))).ToList();

            if (_neighborhoodSystems.Count == 0)
            {
                throw new ArgumentException("At least one neighborhood system is required.", nameof(neighborhoodSystems));
            }

            var duplicatedSystemId = _neighborhoodSystems
                .GroupBy(system => system.Id)
                .FirstOrDefault(group => group.Count() > 1)
                ?.Key;

            if (duplicatedSystemId is not null)
            {
                throw new ArgumentException($"Neighborhood system id '{duplicatedSystemId}' is duplicated.", nameof(neighborhoodSystems));
            }

            _movingObject = movingObject ?? throw new ArgumentNullException(nameof(movingObject));
            _collisionCommandFactory = collisionCommandFactory ?? throw new ArgumentNullException(nameof(collisionCommandFactory));
            _locationProvider = locationProvider ?? (gameObject => new MovingObjectAdapter(gameObject).GetLocation());
            _macroPropertyName = string.IsNullOrWhiteSpace(macroPropertyName)
                ? throw new ArgumentException("Macro property name must not be empty.", nameof(macroPropertyName))
                : macroPropertyName;
        }

        public void Execute()
        {
            var context = new CollisionUpdateContext(_movingObject, _collisionCommandFactory);
            ICommand? chain = null;

            for (var index = _neighborhoodSystems.Count - 1; index >= 0; index--)
            {
                chain = new UpdateNeighborhoodCommand(
                    _neighborhoodSystems[index],
                    context,
                    _locationProvider,
                    chain,
                    replaceMacroCommand: false,
                    macroPropertyName: _macroPropertyName);
            }

            chain?.Execute();

            _movingObject.SetProperty<ICommand>(
                _macroPropertyName,
                new MacroCommand(context.ToCommandList()));
        }
    }
}
