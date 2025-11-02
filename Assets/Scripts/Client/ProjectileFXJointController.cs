using UnityEngine;
using System.Collections.Generic;

namespace ArenaGame.Client
{
    /// <summary>
    /// Controller for ProjectileFX that configures multiple particle joints
    /// spaced evenly around a circle. When the parent rotates, creates
    /// multiple twirling particle emitters.
    /// </summary>
    [ExecuteInEditMode]
    public class ProjectileFXJointController : MonoBehaviour
    {
        [Header("Joint Configuration")]
        [Tooltip("Number of joints to create (spaced evenly around circle)")]
        [Range(1, 16)]
        [SerializeField] private int numberOfJoints = 1;
        
        [Tooltip("Radius from center to place joints")]
        [SerializeField] private float radius = 1f;
        
        [Header("Joint Template")]
        [Tooltip("Template GameObject to clone for each joint. Should contain ParticleJoint1 -> ParticleEmitter structure.")]
        [SerializeField] private GameObject jointTemplate;
        
        [Header("Debug")]
        [Tooltip("Reconfigure joints when this changes")]
        [SerializeField] private bool regenerateJoints = false;
        
        private List<GameObject> createdJoints = new List<GameObject>();
        
        void Awake()
        {
            // Auto-find existing joint template if not assigned
            if (jointTemplate == null)
            {
                FindOrCreateJointTemplate();
            }
        }
        
        void OnValidate()
        {
            // When values change in inspector, regenerate
            if (Application.isPlaying || regenerateJoints)
            {
                RegenerateJoints();
            }
        }
        
        void Start()
        {
            RegenerateJoints();
        }
        
        /// <summary>
        /// Finds existing joint template or creates default structure
        /// </summary>
        private void FindOrCreateJointTemplate()
        {
            // Try to find existing ParticleJoint2
            Transform existingJoint = transform.Find("ParticleJoint2");
            if (existingJoint != null)
            {
                jointTemplate = existingJoint.gameObject;
                Debug.Log($"[ProjectileFXJointController] Found existing ParticleJoint2 template on {gameObject.name}");
                return;
            }
            
            // Try case-insensitive search
            existingJoint = FindChildRecursive(transform, "ParticleJoint2");
            if (existingJoint != null)
            {
                jointTemplate = existingJoint.gameObject;
                Debug.Log($"[ProjectileFXJointController] Found existing ParticleJoint2 template (case-insensitive) on {gameObject.name}");
                return;
            }
            
            Debug.LogWarning($"[ProjectileFXJointController] No joint template found on {gameObject.name}. Create a ParticleJoint2 structure first.");
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
        
        /// <summary>
        /// Regenerates all joints based on current configuration
        /// </summary>
        [ContextMenu("Regenerate Joints")]
        public void RegenerateJoints()
        {
            if (jointTemplate == null)
            {
                FindOrCreateJointTemplate();
                if (jointTemplate == null)
                {
                    Debug.LogError($"[ProjectileFXJointController] Cannot regenerate joints - no template found on {gameObject.name}");
                    return;
                }
            }
            
            // Clean up existing joints (except the template if it's one of them)
            for (int i = createdJoints.Count - 1; i >= 0; i--)
            {
                if (createdJoints[i] != null && createdJoints[i] != jointTemplate)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(createdJoints[i]);
                    }
                    else
                    {
                        DestroyImmediate(createdJoints[i]);
                    }
                }
            }
            createdJoints.Clear();
            
            // Calculate angle step (360 degrees / number of joints)
            float angleStep = 360f / numberOfJoints;
            
            // Create joints
            for (int i = 0; i < numberOfJoints; i++)
            {
                float angle = i * angleStep;
                float radians = angle * Mathf.Deg2Rad;
                
                // Calculate position on circle
                Vector3 position = new Vector3(
                    Mathf.Cos(radians) * radius,
                    0f,
                    Mathf.Sin(radians) * radius
                );
                
                GameObject joint;
                
                // If first joint and template exists, use it if it's at origin
                if (i == 0 && jointTemplate.transform.localPosition.magnitude < 0.01f)
                {
                    // Use template as first joint
                    joint = jointTemplate;
                    joint.name = "ParticleJoint2";
                    createdJoints.Add(joint);
                }
                else
                {
                    // Clone template for additional joints
                    joint = Instantiate(jointTemplate, transform);
                    joint.name = $"ParticleJoint2_{i + 1}";
                    createdJoints.Add(joint);
                }
                
                // Set position
                joint.transform.localPosition = position;
                joint.transform.localRotation = Quaternion.identity;
                joint.transform.localScale = Vector3.one;
            }
            
            Debug.Log($"[ProjectileFXJointController] Generated {numberOfJoints} joints on {gameObject.name} at radius {radius}");
        }
        
        /// <summary>
        /// Gets the current number of joints
        /// </summary>
        public int GetJointCount()
        {
            return numberOfJoints;
        }
        
        /// <summary>
        /// Sets the number of joints and regenerates
        /// </summary>
        public void SetJointCount(int count)
        {
            numberOfJoints = Mathf.Clamp(count, 1, 16);
            RegenerateJoints();
        }
        
        /// <summary>
        /// Sets the radius and regenerates joints
        /// </summary>
        public void SetRadius(float newRadius)
        {
            radius = newRadius;
            RegenerateJoints();
        }
    }
}

