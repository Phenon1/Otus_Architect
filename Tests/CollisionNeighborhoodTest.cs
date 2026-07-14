using CommandsProj;
using CommandsProj.Commands.CollisionCommands;
using ModelsProj;
using ModelsProj.Classes;
using ModelsProj.TypesObject;

namespace Tests
{
    public class CollisionNeighborhoodTest
    {
        private static SpaceShip ShipAt(int x, int y)
        {
            var ship = new SpaceShip();
            ship.SetProperty("Location", new Point(x, y));
            return ship;
        }

        private static void PlaceAll(NeighborhoodSystem system, params IUObject[] objects)
        {
            foreach (var gameObject in objects)
            {
                system.PlaceObject(gameObject, gameObject.GetProperty<Point>("Location"));
            }
        }

        private static CollisionCommandFactory RecordingFactory(
            List<(IUObject First, IUObject Second)> createdPairs,
            List<(IUObject First, IUObject Second)> executedPairs)
        {
            return (first, second) =>
            {
                createdPairs.Add((first, second));
                return new RecordingCollisionCommand(first, second, executedPairs);
            };
        }

        [Test]
        public void NeighborhoodSystemDeterminesCellsWithBoundariesOffsetsAndNegativeCoordinates()
        {
            var system = new NeighborhoodSystem("main", 10, 10);
            var shiftedSystem = new NeighborhoodSystem("shifted", 10, 10, offsetX: 5, offsetY: 5);

            Assert.That(system.GetCell(new Point(0, 0)), Is.EqualTo(new NeighborhoodCell(0, 0)));
            Assert.That(system.GetCell(new Point(9, 9)), Is.EqualTo(new NeighborhoodCell(0, 0)));
            Assert.That(system.GetCell(new Point(10, 10)), Is.EqualTo(new NeighborhoodCell(1, 1)));
            Assert.That(system.GetCell(new Point(-1, -1)), Is.EqualTo(new NeighborhoodCell(-1, -1)));
            Assert.That(system.GetCell(new Point(-10, -10)), Is.EqualTo(new NeighborhoodCell(-1, -1)));
            Assert.That(system.GetCell(new Point(-11, -11)), Is.EqualTo(new NeighborhoodCell(-2, -2)));
            Assert.That(shiftedSystem.GetCell(new Point(4, 4)), Is.EqualTo(new NeighborhoodCell(-1, -1)));
            Assert.That(shiftedSystem.GetCell(new Point(5, 5)), Is.EqualTo(new NeighborhoodCell(0, 0)));
        }

        [Test]
        public void UpdateNeighborhoodCommandAddsObjectToNeighborhoodAndCreatesEmptyMacroWhenThereAreNoCandidates()
        {
            var movingObject = ShipAt(1, 1);
            var system = new NeighborhoodSystem("main", 10, 10);
            var createdPairs = new List<(IUObject First, IUObject Second)>();
            var executedPairs = new List<(IUObject First, IUObject Second)>();
            var context = new CollisionUpdateContext(movingObject, RecordingFactory(createdPairs, executedPairs));

            new UpdateNeighborhoodCommand(system, context).Execute();

            Assert.That(system.ContainsObjectInCell(movingObject, new NeighborhoodCell(0, 0)), Is.True);
            Assert.That(createdPairs, Is.Empty);

            var macroCommand = movingObject.GetProperty<ICommand>(CollisionNeighborhoodConstants.CollisionMacroPropertyName);
            macroCommand.Execute();

            Assert.That(executedPairs, Is.Empty);
        }

        [Test]
        public void UpdateNeighborhoodCommandMovesObjectFromOldNeighborhoodToNewNeighborhood()
        {
            var movingObject = ShipAt(1, 1);
            var oldNeighbor = ShipAt(2, 2);
            var newNeighbor = ShipAt(11, 1);
            var system = new NeighborhoodSystem("main", 10, 10);
            PlaceAll(system, movingObject, oldNeighbor, newNeighbor);

            movingObject.SetProperty("Location", new Point(12, 1));

            var createdPairs = new List<(IUObject First, IUObject Second)>();
            var executedPairs = new List<(IUObject First, IUObject Second)>();
            var context = new CollisionUpdateContext(movingObject, RecordingFactory(createdPairs, executedPairs));

            new UpdateNeighborhoodCommand(system, context).Execute();

            Assert.That(system.ContainsObjectInCell(movingObject, new NeighborhoodCell(0, 0)), Is.False);
            Assert.That(system.ContainsObjectInCell(movingObject, new NeighborhoodCell(1, 0)), Is.True);
            Assert.That(createdPairs, Has.Count.EqualTo(1));
            Assert.That(createdPairs.Single().Second, Is.SameAs(newNeighbor));
        }

        [Test]
        public void UpdateNeighborhoodCommandCreatesCollisionCommandsForEveryOtherObjectInCurrentNeighborhood()
        {
            var movingObject = ShipAt(1, 1);
            var firstNeighbor = ShipAt(2, 2);
            var secondNeighbor = ShipAt(3, 3);
            var farObject = ShipAt(21, 21);
            var system = new NeighborhoodSystem("main", 10, 10);
            PlaceAll(system, firstNeighbor, secondNeighbor, farObject);

            var createdPairs = new List<(IUObject First, IUObject Second)>();
            var executedPairs = new List<(IUObject First, IUObject Second)>();
            var context = new CollisionUpdateContext(movingObject, RecordingFactory(createdPairs, executedPairs));

            new UpdateNeighborhoodCommand(system, context).Execute();

            Assert.That(createdPairs.Select(pair => pair.Second), Is.EquivalentTo(new[] { firstNeighbor, secondNeighbor }));
            Assert.That(createdPairs.Any(pair => ReferenceEquals(pair.Second, movingObject)), Is.False);

            movingObject.GetProperty<ICommand>(CollisionNeighborhoodConstants.CollisionMacroPropertyName).Execute();

            Assert.That(executedPairs.Select(pair => pair.Second), Is.EquivalentTo(new[] { firstNeighbor, secondNeighbor }));
        }

        [Test]
        public void UpdateNeighborhoodCommandReplacesMacroCommandOnEveryExecution()
        {
            var movingObject = ShipAt(1, 1);
            var neighbor = ShipAt(2, 2);
            var system = new NeighborhoodSystem("main", 10, 10);
            PlaceAll(system, neighbor);

            var createdPairs = new List<(IUObject First, IUObject Second)>();
            var executedPairs = new List<(IUObject First, IUObject Second)>();
            var context = new CollisionUpdateContext(movingObject, RecordingFactory(createdPairs, executedPairs));
            var command = new UpdateNeighborhoodCommand(system, context);

            command.Execute();

            movingObject.GetProperty<ICommand>(CollisionNeighborhoodConstants.CollisionMacroPropertyName).Execute();

            Assert.That(createdPairs, Has.Count.EqualTo(1));
            Assert.That(executedPairs, Has.Count.EqualTo(1));

            movingObject.SetProperty("Location", new Point(50, 50));
            executedPairs.Clear();

            command.Execute();
            movingObject.GetProperty<ICommand>(CollisionNeighborhoodConstants.CollisionMacroPropertyName).Execute();

            Assert.That(createdPairs, Has.Count.EqualTo(1));
            Assert.That(executedPairs, Is.Empty);
        }

        [Test]
        public void UpdateCollisionNeighborhoodsCommandFindsCollisionCandidateThroughShiftedNeighborhoodSystem()
        {
            var movingObject = ShipAt(9, 5);
            var boundaryNeighbor = ShipAt(10, 5);
            var mainSystem = new NeighborhoodSystem("main", 10, 10);
            var shiftedSystem = new NeighborhoodSystem("shifted", 10, 10, offsetX: 5, offsetY: 5);
            PlaceAll(mainSystem, boundaryNeighbor);
            PlaceAll(shiftedSystem, boundaryNeighbor);

            var createdPairs = new List<(IUObject First, IUObject Second)>();
            var executedPairs = new List<(IUObject First, IUObject Second)>();

            new UpdateCollisionNeighborhoodsCommand(
                new[] { mainSystem, shiftedSystem },
                movingObject,
                RecordingFactory(createdPairs, executedPairs)).Execute();

            Assert.That(mainSystem.GetCell(movingObject.GetProperty<Point>("Location")), Is.Not.EqualTo(mainSystem.GetCell(boundaryNeighbor.GetProperty<Point>("Location"))));
            Assert.That(shiftedSystem.GetCell(movingObject.GetProperty<Point>("Location")), Is.EqualTo(shiftedSystem.GetCell(boundaryNeighbor.GetProperty<Point>("Location"))));
            Assert.That(createdPairs, Has.Count.EqualTo(1));
            Assert.That(createdPairs.Single().Second, Is.SameAs(boundaryNeighbor));
        }

        [Test]
        public void UpdateCollisionNeighborhoodsCommandDoesNotDuplicateSamePairFoundInSeveralSystems()
        {
            var movingObject = ShipAt(1, 1);
            var neighbor = ShipAt(2, 2);
            var mainSystem = new NeighborhoodSystem("main", 10, 10);
            var shiftedSystem = new NeighborhoodSystem("shifted", 10, 10, offsetX: 5, offsetY: 5);
            PlaceAll(mainSystem, neighbor);
            PlaceAll(shiftedSystem, neighbor);

            var createdPairs = new List<(IUObject First, IUObject Second)>();
            var executedPairs = new List<(IUObject First, IUObject Second)>();

            new UpdateCollisionNeighborhoodsCommand(
                new[] { mainSystem, shiftedSystem },
                movingObject,
                RecordingFactory(createdPairs, executedPairs)).Execute();

            Assert.That(createdPairs, Has.Count.EqualTo(1));

            movingObject.GetProperty<ICommand>(CollisionNeighborhoodConstants.CollisionMacroPropertyName).Execute();

            Assert.That(executedPairs, Has.Count.EqualTo(1));
            Assert.That(executedPairs.Single().Second, Is.SameAs(neighbor));
        }

        [Test]
        public void UpdateCollisionNeighborhoodsCommandSupportsArbitraryNumberOfNeighborhoodSystems()
        {
            var movingObject = ShipAt(9, 5);
            var mainNeighbor = ShipAt(1, 5);
            var shiftedNeighbor = ShipAt(10, 5);
            var thirdSystemNeighbor = ShipAt(14, 5);
            var mainSystem = new NeighborhoodSystem("main", 10, 10);
            var shiftedSystem = new NeighborhoodSystem("shifted", 10, 10, offsetX: 5, offsetY: 5);
            var thirdSystem = new NeighborhoodSystem("third", 10, 10, offsetX: -5, offsetY: 0);

            foreach (var system in new[] { mainSystem, shiftedSystem, thirdSystem })
            {
                PlaceAll(system, mainNeighbor, shiftedNeighbor, thirdSystemNeighbor);
            }

            var createdPairs = new List<(IUObject First, IUObject Second)>();
            var executedPairs = new List<(IUObject First, IUObject Second)>();

            new UpdateCollisionNeighborhoodsCommand(
                new[] { mainSystem, shiftedSystem, thirdSystem },
                movingObject,
                RecordingFactory(createdPairs, executedPairs)).Execute();

            Assert.That(createdPairs.Select(pair => pair.Second), Is.EquivalentTo(new[] { mainNeighbor, shiftedNeighbor, thirdSystemNeighbor }));
        }

        [Test]
        public void CollisionNeighborhoodCommandsValidateArguments()
        {
            var movingObject = ShipAt(0, 0);
            var createdPairs = new List<(IUObject First, IUObject Second)>();
            var executedPairs = new List<(IUObject First, IUObject Second)>();

            Assert.Throws<ArgumentException>(() => new NeighborhoodSystem("", 10, 10));
            Assert.Throws<ArgumentOutOfRangeException>(() => new NeighborhoodSystem("main", 0, 10));
            Assert.Throws<ArgumentOutOfRangeException>(() => new NeighborhoodSystem("main", 10, -1));
            Assert.Throws<ArgumentException>(() => new UpdateCollisionNeighborhoodsCommand(
                Array.Empty<NeighborhoodSystem>(),
                movingObject,
                RecordingFactory(createdPairs, executedPairs)));
            Assert.Throws<ArgumentException>(() => new UpdateCollisionNeighborhoodsCommand(
                new[] { new NeighborhoodSystem("duplicate", 10, 10), new NeighborhoodSystem("duplicate", 20, 20) },
                movingObject,
                RecordingFactory(createdPairs, executedPairs)));
            Assert.Throws<ArgumentNullException>(() => new CollisionUpdateContext(movingObject, null!));
            Assert.Throws<ArgumentException>(() => new UpdateNeighborhoodCommand(
                new NeighborhoodSystem("main", 10, 10),
                new CollisionUpdateContext(movingObject, RecordingFactory(createdPairs, executedPairs)),
                macroPropertyName: ""));
        }

        private sealed class RecordingCollisionCommand : ICommand
        {
            private readonly IUObject _first;
            private readonly IUObject _second;
            private readonly List<(IUObject First, IUObject Second)> _executedPairs;

            public RecordingCollisionCommand(
                IUObject first,
                IUObject second,
                List<(IUObject First, IUObject Second)> executedPairs)
            {
                _first = first;
                _second = second;
                _executedPairs = executedPairs;
            }

            public void Execute()
            {
                _executedPairs.Add((_first, _second));
            }
        }
    }
}
