using UnityEngine;

namespace ArenaGame.Client
{
    /// <summary>
    /// Setup script to parent forward indicator to projectile
    /// Runs in editor and play mode
    /// </summary>
    public class ProjectileSetup : MonoBehaviour
    {
        [ContextMenu("Setup Forward Indicator")]
        void SetupForwardIndicator()
        {
            // Find the forward indicator and parent it to this object
            GameObject forwardIndicator = GameObject.Find("ForwardIndicator");
            if (forwardIndicator != null)
            {
                forwardIndicator.transform.SetParent(transform);
                forwardIndicator.transform.localPosition = new Vector3(0, 0, 0.5f);
                forwardIndicator.transform.localRotation = Quaternion.identity;
                forwardIndicator.transform.localScale = new Vector3(0.5f, 0.5f, 1f); // Make cone point forward (smaller scale, longer in Z)
                
                Debug.Log($"[ProjectileSetup] Parented ForwardIndicator to {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"[ProjectileSetup] ForwardIndicator not found in scene!");
            }
        }
        
        void Awake()
        {
            // Auto-setup in play mode
            if (Application.isPlaying)
            {
                SetupForwardIndicator();
            }
        }
    }
}

