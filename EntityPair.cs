using System;
using Unity.Entities;

namespace Arc.FilteredCollisions
{
    public struct EntityPair : IEquatable<EntityPair>
    {
        public Entity EntityA;
        public Entity EntityB;

        public bool Equals(EntityPair other)
        {
            return EntityA.Equals(other.EntityA) && EntityB.Equals(other.EntityB);
        }

        public override bool Equals(object obj)
        {
            return obj is EntityPair other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EntityA.GetHashCode() * 397) ^ EntityB.GetHashCode();
            }
        }
    }
}