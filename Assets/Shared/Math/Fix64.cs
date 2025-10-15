using System;
using System.Runtime.CompilerServices;

namespace ArenaGame.Shared.Math
{
    /// <summary>
    /// Simple 64-bit fixed-point number (32.32 format)
    /// Deterministic across all platforms
    /// </summary>
    public struct Fix64 : IEquatable<Fix64>, IComparable<Fix64>
    {
        private const int FRACTIONAL_BITS = 16;
        private const long ONE = 1L << FRACTIONAL_BITS;
        private const long HALF = ONE >> 1;
        
        private readonly long rawValue;

        private Fix64(long raw)
        {
            rawValue = raw;
        }

        // Constants
        public static readonly Fix64 Zero = new Fix64(0);
        public static readonly Fix64 One = new Fix64(ONE);
        public static readonly Fix64 Half = new Fix64(HALF);
        public static readonly Fix64 MinValue = new Fix64(long.MinValue);
        public static readonly Fix64 MaxValue = new Fix64(long.MaxValue);

        // Creation
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64 FromInt(int value)
        {
            return new Fix64((long)value << FRACTIONAL_BITS);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64 FromLong(long value)
        {
            return new Fix64(value << FRACTIONAL_BITS);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64 FromFloat(float value)
        {
            return new Fix64((long)(value * ONE));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64 FromDouble(double value)
        {
            return new Fix64((long)(value * ONE));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64 FromRaw(long raw)
        {
            return new Fix64(raw);
        }

        // Conversion to primitives
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ToInt()
        {
            return (int)(rawValue >> FRACTIONAL_BITS);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ToLong()
        {
            return rawValue >> FRACTIONAL_BITS;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ToFloat()
        {
            return (float)rawValue / ONE;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ToDouble()
        {
            return (double)rawValue / ONE;
        }

        public long RawValue => rawValue;

        // Arithmetic operators
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64 operator +(Fix64 a, Fix64 b)
        {
            return new Fix64(a.rawValue + b.rawValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64 operator -(Fix64 a, Fix64 b)
        {
            return new Fix64(a.rawValue - b.rawValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64 operator -(Fix64 a)
        {
            return new Fix64(-a.rawValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64 operator *(Fix64 a, Fix64 b)
        {
            return new Fix64((a.rawValue * b.rawValue) >> FRACTIONAL_BITS);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64 operator /(Fix64 a, Fix64 b)
        {
            return new Fix64((a.rawValue << FRACTIONAL_BITS) / b.rawValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64 operator %(Fix64 a, Fix64 b)
        {
            return new Fix64(a.rawValue % b.rawValue);
        }

        // Comparison operators
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Fix64 a, Fix64 b)
        {
            return a.rawValue == b.rawValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Fix64 a, Fix64 b)
        {
            return a.rawValue != b.rawValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Fix64 a, Fix64 b)
        {
            return a.rawValue < b.rawValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Fix64 a, Fix64 b)
        {
            return a.rawValue > b.rawValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Fix64 a, Fix64 b)
        {
            return a.rawValue <= b.rawValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Fix64 a, Fix64 b)
        {
            return a.rawValue >= b.rawValue;
        }

        // Math functions
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64 Abs(Fix64 value)
        {
            return value.rawValue < 0 ? new Fix64(-value.rawValue) : value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64 Min(Fix64 a, Fix64 b)
        {
            return a.rawValue < b.rawValue ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64 Max(Fix64 a, Fix64 b)
        {
            return a.rawValue > b.rawValue ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64 Clamp(Fix64 value, Fix64 min, Fix64 max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        // Square root using Newton-Raphson method
        public static Fix64 Sqrt(Fix64 value)
        {
            if (value.rawValue < 0)
                return Zero;
            if (value.rawValue == 0)
                return Zero;

            long num = value.rawValue;
            long result = num;
            long lastResult;

            // Newton-Raphson iteration
            for (int i = 0; i < 8; i++)
            {
                lastResult = result;
                result = (result + num / result) >> 1;
                if (result == lastResult)
                    break;
            }

            return new Fix64(result << (FRACTIONAL_BITS / 2));
        }

        // Lerp
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64 Lerp(Fix64 a, Fix64 b, Fix64 t)
        {
            return a + (b - a) * Clamp(t, Zero, One);
        }

        // Interface implementations
        public bool Equals(Fix64 other)
        {
            return rawValue == other.rawValue;
        }

        public override bool Equals(object obj)
        {
            return obj is Fix64 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return rawValue.GetHashCode();
        }

        public int CompareTo(Fix64 other)
        {
            return rawValue.CompareTo(other.rawValue);
        }

        public override string ToString()
        {
            return ToDouble().ToString("F4");
        }

        // Implicit conversions from primitives
        public static implicit operator Fix64(int value) => FromInt(value);
        public static implicit operator Fix64(long value) => FromLong(value);
        
        // Explicit conversions from float/double (to be clear about precision loss)
        public static explicit operator Fix64(float value) => FromFloat(value);
        public static explicit operator Fix64(double value) => FromDouble(value);
        
        // Explicit conversions to primitives
        public static explicit operator int(Fix64 value) => value.ToInt();
        public static explicit operator long(Fix64 value) => value.ToLong();
        public static explicit operator float(Fix64 value) => value.ToFloat();
        public static explicit operator double(Fix64 value) => value.ToDouble();
    }
}

