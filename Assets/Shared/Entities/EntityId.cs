using System;

namespace ArenaGame.Shared.Entities
{
    /// <summary>
    /// Unique identifier for entities in the simulation
    /// </summary>
    public struct EntityId : IEquatable<EntityId>
    {
        public readonly int Value;

        public EntityId(int value)
        {
            Value = value;
        }

        public static readonly EntityId Invalid = new EntityId(-1);

        public bool IsValid => Value >= 0;

        public bool Equals(EntityId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is EntityId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => $"Entity({Value})";

        public static bool operator ==(EntityId a, EntityId b) => a.Value == b.Value;
        public static bool operator !=(EntityId a, EntityId b) => a.Value != b.Value;
    }
}

