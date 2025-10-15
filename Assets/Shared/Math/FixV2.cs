using System;
using System.Runtime.CompilerServices;

namespace ArenaGame.Shared.Math
{
    /// <summary>
    /// 2D vector using deterministic fixed-point math
    /// </summary>
    public struct FixV2 : IEquatable<FixV2>
    {
        public Fix64 X;
        public Fix64 Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixV2(Fix64 x, Fix64 y)
        {
            X = x;
            Y = y;
        }

        // Constants
        public static readonly FixV2 Zero = new FixV2(Fix64.Zero, Fix64.Zero);
        public static readonly FixV2 One = new FixV2(Fix64.One, Fix64.One);
        public static readonly FixV2 Up = new FixV2(Fix64.Zero, Fix64.One);
        public static readonly FixV2 Down = new FixV2(Fix64.Zero, -Fix64.One);
        public static readonly FixV2 Left = new FixV2(-Fix64.One, Fix64.Zero);
        public static readonly FixV2 Right = new FixV2(Fix64.One, Fix64.Zero);

        // Properties
        public Fix64 SqrMagnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => X * X + Y * Y;
        }

        public Fix64 Magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Fix64.Sqrt(SqrMagnitude);
        }

        public FixV2 Normalized
        {
            get
            {
                Fix64 mag = Magnitude;
                return mag > Fix64.Zero ? new FixV2(X / mag, Y / mag) : Zero;
            }
        }

        // Static methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64 Distance(FixV2 a, FixV2 b)
        {
            return (b - a).Magnitude;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64 SqrDistance(FixV2 a, FixV2 b)
        {
            return (b - a).SqrMagnitude;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64 Dot(FixV2 a, FixV2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixV2 Lerp(FixV2 a, FixV2 b, Fix64 t)
        {
            return new FixV2(
                Fix64.Lerp(a.X, b.X, t),
                Fix64.Lerp(a.Y, b.Y, t)
            );
        }

        // Operators
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixV2 operator +(FixV2 a, FixV2 b)
        {
            return new FixV2(a.X + b.X, a.Y + b.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixV2 operator -(FixV2 a, FixV2 b)
        {
            return new FixV2(a.X - b.X, a.Y - b.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixV2 operator -(FixV2 a)
        {
            return new FixV2(-a.X, -a.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixV2 operator *(FixV2 a, Fix64 s)
        {
            return new FixV2(a.X * s, a.Y * s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixV2 operator *(Fix64 s, FixV2 a)
        {
            return new FixV2(a.X * s, a.Y * s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixV2 operator /(FixV2 a, Fix64 s)
        {
            return new FixV2(a.X / s, a.Y / s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(FixV2 a, FixV2 b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(FixV2 a, FixV2 b)
        {
            return !(a == b);
        }

        // Interface implementations
        public bool Equals(FixV2 other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is FixV2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ (Y.GetHashCode() << 2);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        // Conversion helpers
        public static FixV2 FromFloat(float x, float y)
        {
            return new FixV2(Fix64.FromFloat(x), Fix64.FromFloat(y));
        }

        public static FixV2 FromInt(int x, int y)
        {
            return new FixV2(Fix64.FromInt(x), Fix64.FromInt(y));
        }
    }
}

