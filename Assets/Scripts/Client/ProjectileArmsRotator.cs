using UnityEngine;
using System.Collections.Generic;

namespace ArenaGame.Client
{
    /// <summary>
    /// Rotates joint2 about the z axis at a specified rate
    /// References children arms for future use
    /// DEPRECATED: Use ProjectileFXController instead
    /// </summary>
    [System.Obsolete("Use ProjectileFXController instead")]
    public class ProjectileArmsRotator : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("List of arm GameObjects (children)")]
        public List<Transform> arms = new List<Transform>();
        
        [Tooltip("The joint to rotate (joint2)")]
        public Transform joint2;
        
        [Header("Rotation Settings")]
        [Tooltip("Rotation speed in degrees per second")]
        public float rotationSpeed = 2000f;
        
        void Awake()
        {
            // Auto-find arms children if not set
            if (arms.Count == 0)
            {
                FindArms();
            }
            
            // Auto-find joint2 if not set
            if (joint2 == null)
            {
                joint2 = transform.Find("joint2");
                if (joint2 == null)
                {
                    // Try to find any child with "joint2" in the name (case-insensitive)
                    FindJoint2();
                }
            }
            
            if (joint2 == null)
            {
                Debug.LogWarning($"[ProjectileArmsRotator] joint2 not found on {gameObject.name}! Rotation will not work.");
            }
        }
        
        void FindArms()
        {
            // Find all children that might be arms
            foreach (Transform child in transform)
            {
                if (child.name.ToLower().Contains("arm"))
                {
                    arms.Add(child);
                }
            }
            
            if (arms.Count == 0)
            {
                // If no arms found, add all children (in case they're named differently)
                foreach (Transform child in transform)
                {
                    arms.Add(child);
                }
            }
            
            Debug.Log($"[ProjectileArmsRotator] Found {arms.Count} arm(s) on {gameObject.name}");
        }
        
        void FindJoint2()
        {
            // Search recursively for joint2
            joint2 = FindChildRecursive(transform, "joint2");
            
            if (joint2 == null)
            {
                // Try case-insensitive search
                foreach (Transform child in GetComponentsInChildren<Transform>())
                {
                    if (child.name.ToLower().Contains("joint2"))
                    {
                        joint2 = child;
                        break;
                    }
                }
            }
        }
        
        Transform FindChildRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
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
            if (joint2 != null)
            {
                // Rotate joint2 about z axis
                float rotationAmount = rotationSpeed * Time.deltaTime;
                joint2.Rotate(0f, 0f, rotationAmount, Space.Self);
            }
        }
    }
}

