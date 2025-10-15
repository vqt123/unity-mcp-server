using System;
using System.Runtime.CompilerServices;

namespace ArenaGame.Shared.Math
{
    /// <summary>
    /// 3D vector using deterministic fixed-point math
    /// </summary>
    public struct FixV3 : IEquatable<FixV3>
    {
        public Fix64 X;
        public Fix64 Y;
        public Fix64 Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixV3(Fix64 x, Fix64 y, Fix64 z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        // Constants
        public static readonly FixV3 Zero = new FixV3(Fix64.Zero, Fix64.Zero, Fix64.Zero);
        public static readonly FixV3 One = new FixV3(Fix64.One, Fix64.One, Fix64.One);
        public static readonly FixV3 Up = new FixV3(Fix64.Zero, Fix64.One, Fix64.Zero);
        public static readonly FixV3 Down = new FixV3(Fix64.Zero, -Fix64.One, Fix64.Zero);
        public static readonly FixV3 Left = new FixV3(-Fix64.One, Fix64.Zero, Fix64.Zero);
        public static readonly FixV3 Right = new FixV3(Fix64.One, Fix64.Zero, Fix64.Zero);
        public static readonly FixV3 Forward = new FixV3(Fix64.Zero, Fix64.Zero, Fix64.One);
        public static readonly FixV3 Back = new FixV3(Fix64.Zero, Fix64.Zero, -Fix64.One);

        // Properties
        public Fix64 SqrMagnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => X * X + Y * Y + Z * Z;
        }

        public Fix64 Magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Fix64.Sqrt(SqrMagnitude);
        }

        public FixV3 Normalized
        {
            get
            {
                Fix64 mag = Magnitude;
                return mag > Fix64.Zero ? new FixV3(X / mag, Y / mag, Z / mag) : Zero;
            }
        }

        // Static methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64 Distance(FixV3 a, FixV3 b)
        {
            return (b - a).Magnitude;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64 Dot(FixV3 a, FixV3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixV3 Cross(FixV3 a, FixV3 b)
        {
            return new FixV3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X
            );
        }

        // Operators
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixV3 operator +(FixV3 a, FixV3 b)
        {
            return new FixV3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixV3 operator -(FixV3 a, FixV3 b)
        {
            return new FixV3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixV3 operator -(FixV3 a)
        {
            return new FixV3(-a.X, -a.Y, -a.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixV3 operator *(FixV3 a, Fix64 s)
        {
            return new FixV3(a.X * s, a.Y * s, a.Z * s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixV3 operator *(Fix64 s, FixV3 a)
        {
            return new FixV3(a.X * s, a.Y * s, a.Z * s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixV3 operator /(FixV3 a, Fix64 s)
        {
            return new FixV3(a.X / s, a.Y / s, a.Z / s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(FixV3 a, FixV3 b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(FixV3 a, FixV3 b)
        {
            return !(a == b);
        }

        // Interface implementations
        public bool Equals(FixV3 other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            return obj is FixV3 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ (Y.GetHashCode() << 2) ^ (Z.GetHashCode() >> 2);
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }

        // Conversion helpers
        public static FixV3 FromFloat(float x, float y, float z)
        {
            return new FixV3(Fix64.FromFloat(x), Fix64.FromFloat(y), Fix64.FromFloat(z));
        }

        // Convert to/from 2D (using XZ plane for Unity's ground plane)
        public FixV2 ToFixV2()
        {
            return new FixV2(X, Z);
        }

        public static FixV3 FromFixV2(FixV2 v, Fix64 y = default)
        {
            return new FixV3(v.X, y, v.Y);
        }
    }
}

