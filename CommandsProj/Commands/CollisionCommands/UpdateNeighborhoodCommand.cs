using CommandsProj.Commands;
using ModelsProj;
using ModelsProj.Adapters;
using ModelsProj.Classes;

namespace CommandsProj.Commands.CollisionCommands
{
    public class UpdateNeighborhoodCommand : ICommand
    {
        private readonly NeighborhoodSystem _neighborhoodSystem;
        private readonly CollisionUpdateContext _context;
        private readonly Func<IUObject, Point> _locationProvider;
        private readonly ICommand? _next;
        private readonly bool _replaceMacroCommand;
        private readonly string _macroPropertyName;

        public UpdateNeighborhoodCommand(
            NeighborhoodSystem neighborhoodSystem,
            CollisionUpdateContext context,
            Func<IUObject, Point>? locationProvider = null,
            ICommand? next = null,
            bool replaceMacroCommand = true,
            string macroPropertyName = CollisionNeighborhoodConstants.CollisionMacroPropertyName)
        {
            _neighborhoodSystem = neighborhoodSystem ?? throw new ArgumentNullException(nameof(neighborhoodSystem));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _locationProvider = locationProvider ?? (gameObject => new MovingObjectAdapter(gameObject).GetLocation());
            _next = next;
            _replaceMacroCommand = replaceMacroCommand;
            _macroPropertyName = string.IsNullOrWhiteSpace(macroPropertyName)
                ? throw new ArgumentException("Имя свойства макрокоманды не должно быть пустым.", nameof(macroPropertyName))
                : macroPropertyName;
        }

        public void Execute()
        {
            if (_replaceMacroCommand)
            {
                _context.Reset();
            }

            var currentCell = _neighborhoodSystem.GetCell(_locationProvider(_context.MovingObject));
            _neighborhoodSystem.PlaceObject(_context.MovingObject, currentCell);

            foreach (var candidate in _neighborhoodSystem.GetObjects(currentCell).ToList())
            {
                _context.TryAddCollisionCandidate(candidate);
            }

            _next?.Execute();

            if (_replaceMacroCommand)
            {
                _context.MovingObject.SetProperty<ICommand>(
                    _macroPropertyName,
                    new MacroCommand(_context.ToCommandList()));
            }
        }
    }
}
