using ModelsProj;
using ModelsProj.Classes;

namespace CommandsProj.Commands.CollisionCommands
{
    public class NeighborhoodSystem
    {
        private readonly Dictionary<NeighborhoodCell, List<IUObject>> _objectsByCell = new Dictionary<NeighborhoodCell, List<IUObject>>();
        private readonly Dictionary<IUObject, NeighborhoodCell> _cellByObject = new Dictionary<IUObject, NeighborhoodCell>(UObjectReferenceEqualityComparer.Instance);

        public NeighborhoodSystem(string id, int cellWidth, int cellHeight, double offsetX = 0, double offsetY = 0)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Идентификатор системы окрестностей не должен быть пустым.", nameof(id));
            }

            if (cellWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cellWidth), "Ширина ячейки должна быть положительной.");
            }

            if (cellHeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cellHeight), "Высота ячейки должна быть положительной.");
            }

            Id = id;
            CellWidth = cellWidth;
            CellHeight = cellHeight;
            OffsetX = offsetX;
            OffsetY = offsetY;
        }

        public string Id { get; }

        public int CellWidth { get; }

        public int CellHeight { get; }

        public double OffsetX { get; }

        public double OffsetY { get; }

        public NeighborhoodCell GetCell(Point location)
        {
            ArgumentNullException.ThrowIfNull(location);

            var x = (int)Math.Floor((location.x - OffsetX) / CellWidth);
            var y = (int)Math.Floor((location.y - OffsetY) / CellHeight);

            return new NeighborhoodCell(x, y);
        }

        public bool PlaceObject(IUObject gameObject, Point location)
        {
            ArgumentNullException.ThrowIfNull(gameObject);

            return PlaceObject(gameObject, GetCell(location));
        }

        public bool PlaceObject(IUObject gameObject, NeighborhoodCell newCell)
        {
            ArgumentNullException.ThrowIfNull(gameObject);

            if (_cellByObject.TryGetValue(gameObject, out var oldCell) && oldCell == newCell)
            {
                return false;
            }

            if (_cellByObject.TryGetValue(gameObject, out oldCell))
            {
                RemoveFromCell(gameObject, oldCell);
            }

            if (!_objectsByCell.TryGetValue(newCell, out var bucket))
            {
                bucket = new List<IUObject>();
                _objectsByCell[newCell] = bucket;
            }

            if (!bucket.Any(existing => ReferenceEquals(existing, gameObject)))
            {
                bucket.Add(gameObject);
            }

            _cellByObject[gameObject] = newCell;
            return true;
        }

        public bool RemoveObject(IUObject gameObject)
        {
            ArgumentNullException.ThrowIfNull(gameObject);

            if (!_cellByObject.TryGetValue(gameObject, out var cell))
            {
                return false;
            }

            RemoveFromCell(gameObject, cell);
            _cellByObject.Remove(gameObject);

            return true;
        }

        public bool TryGetObjectCell(IUObject gameObject, out NeighborhoodCell cell)
        {
            ArgumentNullException.ThrowIfNull(gameObject);

            return _cellByObject.TryGetValue(gameObject, out cell);
        }

        public IReadOnlyList<IUObject> GetObjects(NeighborhoodCell cell)
        {
            if (!_objectsByCell.TryGetValue(cell, out var objects))
            {
                return Array.Empty<IUObject>();
            }

            return objects.AsReadOnly();
        }

        public bool ContainsObjectInCell(IUObject gameObject, NeighborhoodCell cell)
        {
            ArgumentNullException.ThrowIfNull(gameObject);

            return _objectsByCell.TryGetValue(cell, out var objects)
                && objects.Any(existing => ReferenceEquals(existing, gameObject));
        }

        public int CountObjectsInCell(NeighborhoodCell cell)
        {
            return _objectsByCell.TryGetValue(cell, out var objects) ? objects.Count : 0;
        }

        private void RemoveFromCell(IUObject gameObject, NeighborhoodCell cell)
        {
            if (!_objectsByCell.TryGetValue(cell, out var bucket))
            {
                return;
            }

            bucket.RemoveAll(existing => ReferenceEquals(existing, gameObject));

            if (bucket.Count == 0)
            {
                _objectsByCell.Remove(cell);
            }
        }
    }
}
