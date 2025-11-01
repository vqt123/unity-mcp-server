# Particle System Best Practices

**Document Type**: Unity Particle System Guide  
**Created**: October 2025  
**Project**: Unity Arena Game

---

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Configuration System](#configuration-system)
4. [Common Patterns](#common-patterns)
5. [Troubleshooting](#troubleshooting)
6. [Performance Tips](#performance-tips)

---

## Overview

This document covers best practices for working with Unity Particle Systems in our codebase. Our particle system uses a **configurable settings approach** that allows designers to tune particle effects in the Unity Inspector without touching code.

### Key Principles

1. **Configurability** - All particle settings exposed in Inspector
2. **Separation** - Particle systems as separate GameObjects from entities
3. **Persistence** - Particles continue fading after entity destruction
4. **World Space** - Particles stay where emitted, not attached to moving objects
5. **Fast Fade** - Particles fade out quickly for performance

---

## Architecture

### File Structure

```
Assets/Scripts/Client/
├── ParticleSystemSettings.cs      # Configurable particle settings class
└── EntityVisualizer.cs             # Creates/manages particle systems
```

### Key Classes

#### `ProjectileParticleSettings`
Serializable class that holds all configurable particle parameters. Edit these in the Inspector to tune effects.

**Location**: `Assets/Scripts/Client/ParticleSystemSettings.cs`

#### `EntityVisualizer`
Manages particle system lifecycle:
- Creates particle system as child of projectile
- Detaches particle system when projectile is destroyed
- Ensures particles persist and fade out properly

**Location**: `Assets/Scripts/Client/EntityVisualizer.cs`

---

## Configuration System

### Using Particle Settings

All particle effects use `ProjectileParticleSettings` which can be configured in the Unity Inspector:

```csharp
[Header("Particle Settings")]
[SerializeField] private ProjectileParticleSettings particleSettings = new ProjectileParticleSettings();
```

### Key Settings Groups

#### Main Settings
- **Lifetime**: How long particles live (8-12 seconds for long trails)
- **Speed**: Initial particle velocity (0 = particles stay where emitted)
- **Size**: Particle size range (0.3-0.6 units)
- **Color**: Base particle color (orange fire color)

#### Emission
- **Rate Over Time**: Particles per second (800 for dense trails)
- **Rate Over Distance**: Particles per unit traveled (500 for movement-based)

#### Shape
- **Shape Type**: Circle (point emission)
- **Radius**: Emission point size (0.1 units)

#### Trail Effect
- **Trail Velocity**: How particles trail backward (disabled for stationary particles)
- **Horizontal/Vertical Spread**: Fire flickering effect

#### Color Over Lifetime
- **Start Color**: Bright yellow-white
- **Middle Color**: Orange
- **End Color**: Deep red
- **Alpha Keys**: Fast fade (transparent by 10% of lifetime)

---

## Common Patterns

### 1. Trail Effects (Current Implementation)

**Use Case**: Projectile trails that persist after projectile destruction

**Key Settings**:
- Simulation Space: **World** (particles stay where emitted)
- Initial Speed: **0** (particles don't move)
- Lifetime: **8-12 seconds** (long trails)
- Emission: **High** (800/sec, 500/unit)
- Fade: **Very fast** (transparent by 10% of lifetime)

**Pattern**:
```csharp
// Particles emit from projectile
particleSystem.transform.SetParent(projectile.transform);

// When projectile destroyed, detach particle system
particleSystem.transform.SetParent(null);
particleSystem.emission.enabled = false; // Stop emitting
// Particles continue fading out
```

### 2. Emitter Effects

**Use Case**: Effects that emit continuously from an object

**Key Settings**:
- Simulation Space: **Local** or **World** (depending on needs)
- Initial Speed: **Non-zero** (particles move)
- Lifetime: **Short** (0.5-2 seconds)
- Emission: **Continuous** (rateOverTime only)

### 3. Burst Effects

**Use Case**: One-time explosion or impact effects

**Key Settings**:
- Emission: **Burst only** (rateOverTime = 0)
- Lifetime: **Short** (0.3-1 second)
- Initial Speed: **High** (particles spread out)

---

## Troubleshooting

### Particles Appear White ✅ RESOLVED

**Problem**: Particles render as white squares instead of colored particles.

**Solution**:
1. **Shader**: Use `Particles/Standard Unlit` or `Particles/Alpha Blended` shader
2. **Material Tint**: Set material `_BaseColor` or `_TintColor` to white (allows particle colors through)
3. **Start Color**: Use gradient mode for start color
4. **Color Over Lifetime**: Gradient with proper color keys (yellow → orange → red)

```csharp
// Correct: Use gradient for start color
var startColorGradient = new Gradient();
startColorGradient.SetKeys(/* color keys */);
main.startColor = new ParticleSystem.MinMaxGradient(startColorGradient);

// Correct: Material with white tint
material.SetColor("_BaseColor", Color.white); // Allows particle colors through
material.SetColor("_TintColor", Color.white); // For older shaders
```

### Particles Disappear When Entity Destroyed

**Problem**: Particles vanish instantly when parent object is destroyed.

**Solution**: Detach particle system before destroying parent:
```csharp
// Detach BEFORE destroying
particleSystem.transform.SetParent(null);
Destroy(parentObject); // Now safe to destroy
```

### Particles Don't Fade

**Problem**: Particles remain visible at full opacity.

**Solutions**:
1. **Shader Support**: Use alpha-blending shader (`Particles/Alpha Blended`)
2. **Alpha Gradient**: Ensure gradient has alpha keys decreasing to 0
3. **Material Settings**: Enable alpha blending on material
4. **Render Queue**: Set material to transparent queue (3000)

```csharp
// Enable alpha blending
material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
material.renderQueue = 3000; // Transparent queue
```

### Particles Move With Parent

**Problem**: Particles follow the moving object instead of staying where emitted.

**Solution**: Use World simulation space:
```csharp
main.simulationSpace = ParticleSystemSimulationSpace.World;
main.startSpeed = 0f; // Particles don't move
```

### Particles Not Visible

**Problem**: Can't see particles at all.

**Solutions**:
1. **Size**: Increase particle size (0.3-0.6 units minimum)
2. **Emission**: Increase emission rate (500+ particles/sec)
3. **Lifetime**: Increase lifetime to see longer trails
4. **Material**: Ensure material/shader is assigned
5. **Camera**: Check camera culling isn't hiding particles

---

## Performance Tips

### 1. Particle Count Limits

**Best Practice**: Set reasonable `maxParticles` limit

```csharp
main.maxParticles = 3000; // Reasonable limit
```

**Why**: Prevents runaway particle creation that kills performance.

### 2. Fast Fade-Out

**Best Practice**: Particles should fade out quickly (10-20% of lifetime)

**Why**: Reduces overdraw and keeps particle count manageable.

```csharp
// Fast fade: transparent by 10% of lifetime
new GradientAlphaKey(1.0f, 0.0f),
new GradientAlphaKey(0.4f, 0.03f),
new GradientAlphaKey(0.1f, 0.05f),
new GradientAlphaKey(0.0f, 0.1f) // Gone by 10%
```

### 3. Emission Rate

**Best Practice**: Balance density with performance

- **High density trails**: 500-800 particles/sec
- **Medium effects**: 100-300 particles/sec
- **Light effects**: 20-50 particles/sec

### 4. Lifetime Management

**Best Practice**: Destroy particle system after particles fade

```csharp
float maxLifetime = Mathf.Max(lifetimeMin, lifetimeMax);
Destroy(particleSystem, maxLifetime + 1f); // Clean up after fade
```

### 5. Simulation Space

**Best Practice**: Use World space for stationary particles

**Why**: Prevents recalculating positions when parent moves.

```csharp
main.simulationSpace = ParticleSystemSimulationSpace.World;
```

### 6. Disable Unused Modules

**Best Practice**: Only enable modules you actually use

```csharp
// Disable velocity over lifetime if particles should be stationary
velocityOverLifetime.enabled = false;

// Disable inherit velocity if particles shouldn't follow parent
inheritVelocity.enabled = false;
```

---

## Configuration Reference

### Quick Settings Guide

| Effect Type | Lifetime | Speed | Emission Rate | Fade Speed |
|------------|----------|-------|---------------|------------|
| **Long Trail** | 8-12s | 0 | 500-800/sec | 10-20% |
| **Short Trail** | 1-3s | 0 | 200-400/sec | 20-30% |
| **Emitter** | 0.5-2s | 0.5-2 | 50-150/sec | 30-50% |
| **Burst** | 0.3-1s | 2-5 | Burst only | 50-80% |

### Recommended Defaults

For projectile trails (current working implementation):
- **Lifetime**: 8-12 seconds
- **Speed**: 0 (stationary - particles stay where emitted)
- **Size**: 0.3-0.6 units
- **Emission**: 800/sec, 500/unit
- **Fade**: Transparent by 7% of lifetime (ultra-fast fade)
- **Simulation Space**: World (particles stay where emitted, not attached to projectile)
- **Material**: Uses `Particles/Standard Unlit` or `Particles/Alpha Blended` shader
- **Colors**: Yellow-white → Orange → Red gradient (fire trail effect)

---

## Hero-Specific Particle Effects ✅

Different hero types now have unique particle effects for their projectiles:

- **Mage**: Fire trail (yellow → orange → red)
- **DefaultHero**: Ice arrows (cyan → blue)
- **Warrior**: Rock wall (brown → gray, denser, larger particles)

### Implementation

The system automatically selects particle effects based on hero type:
```csharp
// In EntityVisualizer.cs
string heroType = GetHeroTypeFromOwner(evt.OwnerId);
ParticleEffectType effectType = ProjectileParticleSettings.GetEffectTypeForHero(heroType);
ProjectileParticleSettings.ApplyPreset(particles, effectType, direction);
```

### Effect Types

#### Fire Effect (Mage)
- Colors: Bright yellow-white → Orange → Deep red
- Style: Fire trail
- Same settings as base config

#### Ice Effect (DefaultHero)
- Colors: Cyan-white → Light blue → Blue → Dark blue
- Style: Ice arrow trail
- Same settings as base config

#### Rock Effect (Warrior)
- Colors: Light brown → Brown → Gray → Dark gray
- Style: Wall of rocks (denser, larger particles)
- Larger size: 0.4-0.8 units
- Higher emission: 1000/sec, 600/unit
- Wider spread: 0.2 radius

---

## Current Working Configuration ✅

### Projectile Trail Settings (Verified Working)

**Location**: `Assets/Scripts/Client/ParticleSystemSettings.cs`

#### Main Settings
```csharp
lifetimeMin = 8f;          // Long lifetime for visible trails
lifetimeMax = 12f;
speedMin = 0f;             // Stationary particles (stay where emitted)
speedMax = 0f;
sizeMin = 0.3f;            // Visible particle size
sizeMax = 0.6f;
maxParticles = 5000;
```

#### Emission
```csharp
rateOverTime = 800f;       // Dense trail (800 particles/sec)
rateOverDistance = 500f;   // Also emit based on movement
```

#### Shape
```csharp
shapeType = Circle;        // Point emission
shapeRadius = 0.1f;        // Small emission point
```

#### Trail Effect
```csharp
velocityOverLifetime = DISABLED;  // Particles don't move
inheritVelocity = DISABLED;        // Don't follow projectile
simulationSpace = World;           // World space (stay where emitted)
```

#### Colors (Fire Trail Gradient)
```csharp
startColorLifetime = (1, 1, 0.5, 1)    // Bright yellow-white
middleColorLifetime = (1, 0.4, 0, 1)   // Orange
endColorLifetime = (0.8, 0.1, 0, 1)    // Deep red
```

#### Fade Curve (Ultra-Fast)
```
Alpha Keys:
- 0.0f → 1.0 (fully visible)
- 0.015f → 0.3 (start fading)
- 0.03f → 0.1 (fading fast)
- 0.05f → 0.02 (almost gone)
- 0.07f → 0.0 (completely transparent)
```

**Fade Time**: 7% of lifetime (very fast for performance)

#### Material Setup
```csharp
Shader: Particles/Standard Unlit (or Particles/Alpha Blended)
Material Color: White (_BaseColor or _TintColor = white)
Render Mode: Billboard
```

**Key Insight**: Material must use white tint/color so particle system colors show through.

---

## Future Improvements

### Potential Enhancements

1. **Texture Support**: Add particle textures instead of colored squares
2. **Multiple Presets**: Different particle styles (fire, smoke, magic, etc.)
3. **Color Variations**: Randomize colors within fire spectrum
4. **Performance Profiling**: Monitor particle count and adjust dynamically
5. **Visual Preview**: Unity Editor preview of particle settings

---

## Code Examples

### Creating a New Particle Effect

```csharp
// In EntityVisualizer.cs
private void AddProjectileParticles(GameObject projectile, ProjectileSpawnedEvent evt)
{
    GameObject particleObj = new GameObject("ParticleTrail");
    particleObj.transform.SetParent(projectile.transform);
    
    ParticleSystem particles = particleObj.AddComponent<ParticleSystem>();
    
    // Apply settings
    particleSettings.ApplyToParticleSystem(particles, direction);
    
    // Store reference for cleanup
    projectileParticleSystems[evt.ProjectileId] = particles;
}
```

### Detaching Particles on Destroy

```csharp
private void DetachParticleSystem(ParticleSystem particles, EntityId id)
{
    // CRITICAL: Unparent FIRST
    particles.transform.SetParent(null);
    
    // Stop emitting
    particles.emission.enabled = false;
    
    // Auto-destroy after fade
    float maxLifetime = Mathf.Max(lifetimeMin, lifetimeMax);
    Destroy(particles.gameObject, maxLifetime + 1f);
}
```

---

## Resources

- **Unity Particle System Documentation**: https://docs.unity3d.com/Manual/ParticleSystemModules.html
- **Project Settings**: `Assets/Scripts/Client/ParticleSystemSettings.cs`
- **Implementation**: `Assets/Scripts/Client/EntityVisualizer.cs`

---

**Last Updated**: October 2025

