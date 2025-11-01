using UnityEngine;

namespace ArenaGame.Client
{
    /// <summary>
    /// Controls particle effects on projectiles with different modes
    /// </summary>
    public class ProjectileFXController : MonoBehaviour
    {
        public enum FXMode
        {
            Mode1,  // Does nothing
            Mode2   // Rotates particleJoint2 about z axis at 2000 deg/s
        }
        
        [Header("FX Settings")]
        [Tooltip("Current FX mode")]
        public FXMode mode = FXMode.Mode1;
        
        [Header("Mode 2 Settings")]
        [Tooltip("Rotation speed in degrees per second (Mode 2 only)")]
        public float rotationSpeed = 2000f;
        
        [Header("References")]
        [Tooltip("The joint to rotate (particleJoint2) - auto-found if not set")]
        public Transform particleJoint2;
        
        void Awake()
        {
            // Auto-find particleJoint2 if not set
            if (particleJoint2 == null)
            {
                FindParticleJoint2();
            }
            
            if (particleJoint2 == null && mode == FXMode.Mode2)
            {
                Debug.LogWarning($"[ProjectileFXController] particleJoint2 not found on {gameObject.name}! Mode 2 rotation will not work.");
            }
        }
        
        void FindParticleJoint2()
        {
            // Try exact name first
            particleJoint2 = transform.Find("particleJoint2");
            
            if (particleJoint2 == null)
            {
                // Try case-insensitive recursive search
                particleJoint2 = FindChildRecursive(transform, "particleJoint2");
            }
            
            if (particleJoint2 == null)
            {
                // Try alternative names
                particleJoint2 = transform.Find("joint2");
                if (particleJoint2 == null)
                {
                    particleJoint2 = FindChildRecursive(transform, "joint2");
                }
            }
        }
        
        Transform FindChildRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                {
                    return child;
                }
                
                Transform found = FindChildRecursive(child, name);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }
        
        void Update()
        {
            // Mode 1: Do nothing
            if (mode == FXMode.Mode1)
            {
                return;
            }
            
            // Mode 2: Rotate particleJoint2 about z axis
            if (mode == FXMode.Mode2 && particleJoint2 != null)
            {
                float rotationAmount = rotationSpeed * Time.deltaTime;
                particleJoint2.Rotate(0f, 0f, rotationAmount, Space.Self);
            }
        }
    }
}

