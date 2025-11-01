using UnityEngine;

namespace ArenaGame.Client
{
    /// <summary>
    /// Configurable settings for projectile particle effects.
    /// Adjust these in the Unity Inspector to tune particle appearance.
    /// </summary>
    [System.Serializable]
    public class ProjectileParticleSettings
    {
        [Header("Main Settings")]
        [Tooltip("How long each particle lives (seconds) - longer = longer trail")]
        public float lifetimeMin = 8f;
        public float lifetimeMax = 12f;
        
        [Tooltip("How fast particles move initially - 0 = particles stay where emitted")]
        public float speedMin = 0f;
        public float speedMax = 0f;
        
        [Tooltip("Particle size range - bigger = more visible")]
        public float sizeMin = 0.3f;
        public float sizeMax = 0.6f;
        
        [Tooltip("Base color of particles")]
        public Color startColor = new Color(1f, 0.7f, 0.2f, 1f);
        
        [Tooltip("Maximum number of particles in system")]
        public int maxParticles = 5000;
        
        [Header("Emission")]
        [Tooltip("Particles per second - higher = denser trail")]
        public float rateOverTime = 800f;
        
        [Tooltip("Particles per unit of movement - higher = denser trail")]
        public float rateOverDistance = 500f;
        
        [Header("Shape (Trail)")]
        [Tooltip("Emission point radius")]
        public float shapeRadius = 0.1f;
        
        [Header("Trail Effect")]
        [Tooltip("How much particles trail backward - stronger = longer visible trail")]
        public float trailBackwardVelocityMin = -3f;
        public float trailBackwardVelocityMax = -6f;
        
        [Tooltip("Horizontal spread for fire flickering")]
        public float horizontalSpread = 0.2f;
        
        [Tooltip("Vertical spread for fire flickering")]
        public float verticalSpread = 0.2f;
        
        [Header("Color Over Lifetime")]
        [Tooltip("Color at start of particle life")]
        public Color startColorLifetime = new Color(1f, 1f, 0.5f, 1f);
        
        [Tooltip("Color in middle of particle life")]
        public Color middleColorLifetime = new Color(1f, 0.4f, 0f, 1f);
        
        [Tooltip("Color at end of particle life")]
        public Color endColorLifetime = new Color(0.8f, 0.1f, 0f, 1f);
        
        /// <summary>
        /// Apply these settings to a ParticleSystem component
        /// </summary>
        public void ApplyToParticleSystem(ParticleSystem particles, Vector3 direction)
        {
            // Create color gradient for start color (fire colors)
            var startColorGradient = new Gradient();
            startColorGradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(startColorLifetime, 0.0f),
                    new GradientColorKey(startColorLifetime, 1.0f)
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(1.0f, 1.0f)
                }
            );
            
            // Main module
            var main = particles.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetimeMin, lifetimeMax);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
            main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
            // Use gradient for start color so particles have color from the start
            main.startColor = new ParticleSystem.MinMaxGradient(startColorGradient);
            main.maxParticles = maxParticles;
            main.simulationSpace = ParticleSystemSimulationSpace.World; // World space - particles stay where emitted
            main.loop = true;
            main.playOnAwake = true;
            
            // Emission
            var emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = rateOverTime;
            emission.rateOverDistance = rateOverDistance;
            
            // Shape - emit from point (particles stay where emitted)
            var shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = shapeRadius;
            shape.radiusThickness = 0f;
            
            // Velocity over lifetime - DISABLED, particles stay where emitted
            // Particles emit and remain stationary in world space, creating trail as projectile moves
            var velocityOverLifetime = particles.velocityOverLifetime;
            velocityOverLifetime.enabled = false; // No velocity - particles just sit where emitted
            
            // Don't inherit projectile velocity
            var inheritVelocity = particles.inheritVelocity;
            inheritVelocity.enabled = false; // Don't inherit projectile velocity
            
            // Color over lifetime - fire colors with ULTRA fast fade out
            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(startColorLifetime, 0.0f),
                    new GradientColorKey(middleColorLifetime, 0.03f),
                    new GradientColorKey(endColorLifetime, 0.05f),
                    new GradientColorKey(endColorLifetime, 1.0f)
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1.0f, 0.0f),      // Fully visible at start
                    new GradientAlphaKey(0.3f, 0.015f),     // Start fading immediately
                    new GradientAlphaKey(0.1f, 0.03f),      // Fading ultra fast
                    new GradientAlphaKey(0.02f, 0.05f),    // Almost gone
                    new GradientAlphaKey(0.0f, 0.07f)       // Completely transparent by 7% of lifetime
                }
            );
            colorOverLifetime.color = gradient;
            
            // Size over lifetime - particles shrink ultra quickly as they fade
            var sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            var sizeCurve = new AnimationCurve(
                new Keyframe(0f, 1f, 0f, 0f),       // Full size at start
                new Keyframe(0.015f, 0.4f, -2f, -2f),  // Shrink ultra fast
                new Keyframe(0.03f, 0.15f, -0.8f, -0.8f),  // Shrink more
                new Keyframe(0.05f, 0.03f, -0.4f, -0.4f),  // Almost gone
                new Keyframe(0.07f, 0f, -0.1f, -0.1f)      // Disappear by 7%
            );
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
            
            // Limit velocity - keep particles in trail formation (disabled for longer trails)
            var limitVelocityOverLifetime = particles.limitVelocityOverLifetime;
            limitVelocityOverLifetime.enabled = false; // Disable to allow longer trails
            
            // Set renderer with alpha support for fading and COLOR support
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                renderer.alignment = ParticleSystemRenderSpace.View;
                
                // Create material with Particles/Standard Unlit shader (supports colors and alpha)
                // This is the modern Unity particle shader that properly supports particle colors
                Shader particleShader = Shader.Find("Particles/Standard Unlit") ??
                                       Shader.Find("Particles/Unlit") ??
                                       Shader.Find("Particles/Alpha Blended") ??
                                       Shader.Find("Sprites/Default");
                
                if (particleShader != null)
                {
                    Material particleMaterial = new Material(particleShader);
                    // For Particles/Standard Unlit, ensure colors show through
                    // Set base color to white so particle colors aren't overridden
                    if (particleShader.name.Contains("Particles"))
                    {
                        // Particle shaders use _BaseColor or _TintColor
                        particleMaterial.SetColor("_BaseColor", Color.white);
                        particleMaterial.SetColor("_TintColor", Color.white);
                    }
                    else
                    {
                        particleMaterial.SetColor("_Color", Color.white);
                    }
                    renderer.material = particleMaterial;
                }
            }
            
            // Color over lifetime is already set above with proper alpha fade
        }
    }
}

