using ModelsProj;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CommandsProj.Commands.CollisionCommands
{
    public sealed class UObjectReferenceEqualityComparer : IEqualityComparer<IUObject>
    {
        public static readonly UObjectReferenceEqualityComparer Instance = new UObjectReferenceEqualityComparer();

        private UObjectReferenceEqualityComparer()
        {
        }

        public bool Equals(IUObject? x, IUObject? y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode([DisallowNull] IUObject obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
