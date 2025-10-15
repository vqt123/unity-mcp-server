using UnityEngine;
using ArenaGame.Shared.Math;

namespace ArenaGame.Client
{
    /// <summary>
    /// Utility methods for converting between simulation types and Unity types
    /// </summary>
    public static class ConversionUtility
    {
        /// <summary>
        /// Convert FixV2 to Unity Vector3 (Y = 0, on ground plane)
        /// </summary>
        public static Vector3 ToVector3(this FixV2 pos, float yOffset = 0f)
        {
            return new Vector3(
                (float)pos.X.ToDouble(),
                yOffset,
                (float)pos.Y.ToDouble()
            );
        }
        
        /// <summary>
        /// Convert Unity Vector3 to FixV2 (uses x and z)
        /// </summary>
        public static FixV2 ToFixV2(this Vector3 pos)
        {
            return FixV2.FromFloat(pos.x, pos.z);
        }
        
        /// <summary>
        /// Convert Unity Vector2 to FixV2
        /// </summary>
        public static FixV2 ToFixV2(this Vector2 pos)
        {
            return FixV2.FromFloat(pos.x, pos.y);
        }
        
        /// <summary>
        /// Convert FixV2 to Unity Vector2
        /// </summary>
        public static Vector2 ToVector2(this FixV2 pos)
        {
            return new Vector2(
                (float)pos.X.ToDouble(),
                (float)pos.Y.ToDouble()
            );
        }
        
        /// <summary>
        /// Convert Fix64 angle (radians) to Unity Quaternion rotation
        /// </summary>
        public static Quaternion ToRotation(this Fix64 radians)
        {
            float degrees = (float)radians.ToDouble() * Mathf.Rad2Deg;
            return Quaternion.Euler(0f, degrees, 0f);
        }
        
        /// <summary>
        /// Get direction from FixV2
        /// </summary>
        public static Vector3 ToDirection(this FixV2 dir)
        {
            return new Vector3(
                (float)dir.X.ToDouble(),
                0f,
                (float)dir.Y.ToDouble()
            ).normalized;
        }
    }
}

