using UnityEngine;

namespace ArenaGame.Client
{
    /// <summary>
    /// Adds trail renderer to projectiles
    /// </summary>
    [RequireComponent(typeof(TrailRenderer))]
    public class ProjectileTrail : MonoBehaviour
    {
        [SerializeField] private Color trailColor = Color.yellow;
        [SerializeField] private float trailWidth = 0.1f;
        [SerializeField] private float trailTime = 0.3f;
        
        private TrailRenderer trail;
        
        void Awake()
        {
            trail = GetComponent<TrailRenderer>();
            SetupTrail();
        }
        
        private void SetupTrail()
        {
            if (trail == null) return;
            
            trail.time = trailTime;
            trail.startWidth = trailWidth;
            trail.endWidth = 0f;
            trail.startColor = trailColor;
            trail.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.material.color = trailColor;
        }
    }
}

