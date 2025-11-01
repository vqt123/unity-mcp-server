using UnityEngine;

namespace ArenaGame.Client
{
    /// <summary>
    /// Particle effect preset type
    /// </summary>
    public enum ParticleEffectType
    {
        Fire,   // Mage - fire trail
        Ice,    // DefaultHero - ice arrows
        Rock    // Warrior - wall of rocks
    }
    
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
        /// Get particle effect preset for hero type
        /// </summary>
        public static ParticleEffectType GetEffectTypeForHero(string heroType)
        {
            switch (heroType)
            {
                case "Mage":
                    return ParticleEffectType.Fire;
                case "DefaultHero":
                    return ParticleEffectType.Ice;
                case "Warrior":
                    return ParticleEffectType.Rock;
                default:
                    return ParticleEffectType.Ice; // Default to ice
            }
        }
        
        /// <summary>
        /// Apply particle effect preset based on hero type
        /// </summary>
        public static void ApplyPreset(ParticleSystem particles, ParticleEffectType effectType, Vector3 direction)
        {
            switch (effectType)
            {
                case ParticleEffectType.Fire:
                    ApplyFireEffect(particles, direction);
                    break;
                case ParticleEffectType.Ice:
                    ApplyIceEffect(particles, direction);
                    break;
                case ParticleEffectType.Rock:
                    ApplyRockEffect(particles, direction);
                    break;
            }
        }
        
        /// <summary>
        /// Apply fire trail effect (Mage)
        /// </summary>
        private static void ApplyFireEffect(ParticleSystem particles, Vector3 direction)
        {
            var settings = new ProjectileParticleSettings(); // Use current settings as base
            
            // Fire colors
            settings.startColorLifetime = new Color(1f, 1f, 0.5f, 1f); // Bright yellow-white
            settings.middleColorLifetime = new Color(1f, 0.4f, 0f, 1f); // Orange
            settings.endColorLifetime = new Color(0.8f, 0.1f, 0f, 1f); // Deep red
            
            settings.ApplyToParticleSystem(particles, direction);
        }
        
        /// <summary>
        /// Apply ice arrow effect (DefaultHero) - twirly effervescent effect
        /// </summary>
        private static void ApplyIceEffect(ParticleSystem particles, Vector3 direction)
        {
            var main = particles.main;
            // Match ParticleTestPrefab settings exactly
            main.startLifetime = new ParticleSystem.MinMaxCurve(5f); // From prefab: scalar = 5
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f); // From prefab: scalar = 5
            main.startSize = new ParticleSystem.MinMaxCurve(1f); // From prefab: scalar = 1
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f);
            main.useUnscaledTime = false; // From prefab: useUnscaledTime = 0
            main.maxParticles = 100000; // From prefab
            main.simulationSpace = ParticleSystemSimulationSpace.World; // From prefab: moveWithTransform = 0
            main.loop = true; // From prefab: looping = 1
            main.playOnAwake = true; // From prefab: playOnAwake = 1
            main.duration = 5f; // From prefab: lengthInSec = 5
            main.stopAction = ParticleSystemStopAction.None; // From prefab: stopAction = 0
            
            // Start color - Match ParticleTestPrefab settings (white)
            main.startColor = new ParticleSystem.MinMaxGradient(Color.white); // From prefab: white color
            
            // Emission - Match ParticleTestPrefab settings exactly
            var emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(10f); // From prefab: scalar = 10
            emission.rateOverDistance = 0f; // From prefab: scalar = 0
            emission.SetBursts(new ParticleSystem.Burst[0]); // From prefab: m_Bursts = []
            
            // Shape - Match ParticleTestPrefab settings exactly
            var shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle; // From prefab: type = 4
            shape.radius = 1f; // From prefab: radius.value = 1
            shape.radiusThickness = 1f; // From prefab: radiusThickness = 1
            
            // Candy cane swirl - emitter spins, particles stay still
            // All velocity disabled - particles just emit and stay where they are
            // The emitter GameObject will rotate to create the spiral pattern
            var velocityOverLifetime = particles.velocityOverLifetime;
            velocityOverLifetime.enabled = false; // No velocity - particles stay still
            
            var inheritVelocity = particles.inheritVelocity;
            inheritVelocity.enabled = false; // No velocity inheritance
            
            // No rotation over lifetime - particles stay still, emitter rotates
            var rotationOverLifetime = particles.rotationOverLifetime;
            rotationOverLifetime.enabled = false; // Particles don't rotate, emitter does
            
            // No noise - want clean, visible spiral (no interference)
            var noise = particles.noise;
            noise.enabled = false;
            
            // Color over lifetime - Disabled to match prefab
            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = false; // From prefab: ColorModule enabled = 0
            
            // Size over lifetime - Disabled to match prefab
            var sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = false; // From prefab: SizeModule enabled = 0
            
            // Limit velocity
            var limitVelocityOverLifetime = particles.limitVelocityOverLifetime;
            limitVelocityOverLifetime.enabled = false;
            
            // Renderer
            SetupRenderer(particles);
        }
        
        /// <summary>
        /// Apply rock wall effect (Warrior)
        /// </summary>
        private static void ApplyRockEffect(ParticleSystem particles, Vector3 direction)
        {
            var main = particles.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(8f, 12f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0f, 0f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.8f); // Larger rocks
            main.maxParticles = 5000;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.loop = true;
            main.playOnAwake = true;
            
            // Rock colors (brown/gray)
            var rockGradient = new Gradient();
            rockGradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(new Color(0.8f, 0.7f, 0.6f, 1f), 0.0f),  // Light brown
                    new GradientColorKey(new Color(0.6f, 0.5f, 0.4f, 1f), 0.03f),  // Brown
                    new GradientColorKey(new Color(0.4f, 0.4f, 0.4f, 1f), 0.05f),  // Gray
                    new GradientColorKey(new Color(0.3f, 0.3f, 0.3f, 1f), 1.0f)   // Dark gray
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(0.3f, 0.015f),
                    new GradientAlphaKey(0.1f, 0.03f),
                    new GradientAlphaKey(0.02f, 0.05f),
                    new GradientAlphaKey(0.0f, 0.07f)
                }
            );
            main.startColor = new ParticleSystem.MinMaxGradient(rockGradient);
            
            // Emission - denser for "wall" effect
            var emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = 1000f; // More particles for wall
            emission.rateOverDistance = 600f;
            
            // Shape - wider spread for wall effect
            var shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.2f; // Wider spread
            shape.radiusThickness = 0f;
            
            // No velocity - particles stay where emitted
            var velocityOverLifetime = particles.velocityOverLifetime;
            velocityOverLifetime.enabled = false;
            
            var inheritVelocity = particles.inheritVelocity;
            inheritVelocity.enabled = false;
            
            // Color over lifetime
            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = rockGradient;
            
            // Size over lifetime
            var sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            var sizeCurve = new AnimationCurve(
                new Keyframe(0f, 1f, 0f, 0f),
                new Keyframe(0.015f, 0.4f, -2f, -2f),
                new Keyframe(0.03f, 0.15f, -0.8f, -0.8f),
                new Keyframe(0.05f, 0.03f, -0.4f, -0.4f),
                new Keyframe(0.07f, 0f, -0.1f, -0.1f)
            );
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
            
            // Limit velocity
            var limitVelocityOverLifetime = particles.limitVelocityOverLifetime;
            limitVelocityOverLifetime.enabled = false;
            
            // Renderer
            SetupRenderer(particles);
        }
        
        /// <summary>
        /// Setup renderer with proper shader for particle colors
        /// </summary>
        private static void SetupRenderer(ParticleSystem particles)
        {
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                renderer.alignment = ParticleSystemRenderSpace.View;
                
                // Create material with particle shader
                Shader particleShader = Shader.Find("Particles/Standard Unlit") ??
                                       Shader.Find("Particles/Unlit") ??
                                       Shader.Find("Particles/Alpha Blended") ??
                                       Shader.Find("Sprites/Default");
                
                if (particleShader != null)
                {
                    Material particleMaterial = new Material(particleShader);
                    if (particleShader.name.Contains("Particles"))
                    {
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
        }
        
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

