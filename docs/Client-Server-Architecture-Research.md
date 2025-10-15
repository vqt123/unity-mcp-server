# Client-Server Architecture Research Document

## Executive Summary

This document outlines the research and architectural recommendations for transitioning the arena game to a client-server architecture with deterministic simulation. The goal is to create a shared simulation codebase that can run on both client and server, with a clear separation between game logic and presentation.

**Key Recommendations:**
- **Fixed-Point Math Library**: Use `FixedMath.Net` or `Deterministic.FixedPoint` for deterministic calculations
- **Simulation Tick Rate**: 30 ticks per second (33.33ms per tick)
- **Client Architecture**: Snapshot Interpolation + Command Pattern
- **Assembly Structure**: Shared assembly with zero Unity dependencies
- **Visualization Pattern**: Event-driven with interpolated rendering

---

## Table of Contents

1. [Fixed-Point Math Libraries](#fixed-point-math-libraries)
2. [Deterministic Simulation Architecture](#deterministic-simulation-architecture)
3. [Client-Server Communication Patterns](#client-server-communication-patterns)
4. [Recommended Architecture](#recommended-architecture)
5. [Assembly Structure](#assembly-structure)
6. [State Synchronization Strategies](#state-synchronization-strategies)
7. [Performance Considerations](#performance-considerations)
8. [Implementation Roadmap](#implementation-roadmap)
9. [Code Examples](#code-examples)
10. [References and Resources](#references-and-resources)

---

## 1. Fixed-Point Math Libraries

### Why Fixed-Point Math?

Floating-point math is **non-deterministic** across platforms due to:
- Different CPU architectures (x86 vs ARM)
- Compiler optimizations
- Rounding modes
- IEEE 754 implementation variations

For multiplayer games, determinism is critical to ensure:
- Server and client produce identical results
- Replay systems work correctly
- Lockstep synchronization is possible
- Cheating detection is reliable

### Recommended Libraries

#### Option 1: FixedMath.Net (Recommended)
- **GitHub**: asik/FixedMath.Net
- **License**: MIT
- **Popularity**: ~500 stars
- **Pros**:
  - Well-tested, mature library
  - Complete math API (sin, cos, sqrt, etc.)
  - Good performance
  - No external dependencies
  - Simple API similar to `float`
- **Cons**:
  - Not actively maintained (last update 2018)
  - 64-bit only (acceptable for most games)

**Example**:
```csharp
using FixedMath.NET;

Fix64 health = Fix64.FromInt(100);
Fix64 damage = Fix64.FromFloat(15.5f);
Fix64 newHealth = health - damage; // 84.5
```

#### Option 2: Deterministic.FixedPoint
- **Part of**: Unity DOTS NetCode package
- **License**: Unity Companion License
- **Pros**:
  - Officially supported by Unity
  - Optimized for Unity DOTS/ECS
  - Actively maintained
  - Integrated with Unity's multiplayer stack
- **Cons**:
  - Requires Unity packages
  - More complex setup
  - Tied to Unity ecosystem

#### Option 3: SoftFloat
- **Based on**: Berkeley SoftFloat library
- **Pros**:
  - Extremely accurate
  - Industry standard
  - Used in emulators and critical systems
- **Cons**:
  - C library, requires P/Invoke
  - More complex integration
  - Slower than native fixed-point

### Recommendation

**Use FixedMath.Net** for the following reasons:
1. Zero Unity dependencies (works in shared assembly)
2. Simple API, easy to understand and debug
3. Proven in production games
4. Can be copied directly into project (single file)
5. Good performance for 30 tick simulation

---

## 2. Deterministic Simulation Architecture

### Core Principles

1. **Separation of Simulation and Rendering**
   - Simulation runs at fixed 30 ticks/second
   - Rendering runs at variable frame rate (60/120+ fps)
   - Client interpolates between simulation states for smooth visuals

2. **Pure Functions**
   - All simulation code must be deterministic
   - No `Time.time`, `Time.deltaTime`, `Random.value`
   - Use simulation tick counter and seeded random

3. **No Unity Dependencies**
   - Simulation code cannot reference UnityEngine namespace
   - Use custom math types (Fixed64 vectors)
   - Abstract input/output through interfaces

### Simulation Loop Structure

```
┌─────────────────────────────────────────────────┐
│                 Game Loop (Unity)                │
│  - Runs at variable frame rate (60-144 fps)     │
└──────────────────┬──────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────┐
│            Simulation Accumulator                │
│  - Accumulates deltaTime                         │
│  - Triggers fixed ticks (30 ticks/sec)          │
└──────────────────┬──────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────┐
│         Simulation Tick (33.33ms)                │
│  - Process player inputs                         │
│  - Update game state (deterministic)             │
│  - Generate events (shot fired, enemy killed)    │
└──────────────────┬──────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────┐
│         Client Rendering (every frame)           │
│  - Interpolate between tick states              │
│  - Render at 60+ fps                            │
│  - Play animations/effects                       │
└─────────────────────────────────────────────────┘
```

### Why 30 Ticks Per Second?

**Advantages**:
- **Lower bandwidth**: 30 updates/sec vs 60 reduces traffic by 50%
- **Lower CPU**: Server can handle more players
- **Acceptable latency**: 33ms update rate is imperceptible with interpolation
- **Industry standard**: Most AAA shooters use 20-64 tick rate
- **Bandwidth efficiency**: ~10KB/sec per player vs 20KB at 60 tick

**Disadvantages**:
- Slightly higher perceived latency (vs 60 tick)
- Fast-moving projectiles may tunnel through targets (use continuous collision)

**Comparison**:
- Counter-Strike: GO = 64 tick (matchmaking), 128 tick (competitive)
- Overwatch = 60 tick
- Fortnite = 30 tick
- League of Legends = 30 tick
- Valorant = 128 tick

For a top-down arena shooter, **30 tick is optimal** balance of performance and responsiveness.

---

## 3. Client-Server Communication Patterns

### Pattern 1: Snapshot System (State Replication)

**How it works**:
- Server sends full game state every tick (30 times/sec)
- Client receives snapshots and stores them
- Client interpolates between two snapshots for smooth rendering

**Pros**:
- Simple to implement
- Client is always up-to-date
- Handles packet loss well (just use next snapshot)
- Easy to debug (can inspect exact state)

**Cons**:
- High bandwidth (sends everything every tick)
- Redundant data (position updates even if not moving)
- Scales poorly with many entities

**Best for**: 
- Small number of entities (<100)
- Unreliable networks (WiFi, mobile)
- Games where everything moves frequently

**Example Bandwidth** (uncompressed):
```
Per Entity: 
- Position (3x Fix64 = 24 bytes)
- Rotation (4x Fix64 = 32 bytes)  
- Health (Fix64 = 8 bytes)
= 64 bytes per entity

20 entities × 64 bytes × 30 ticks/sec = 38.4 KB/sec per player
```

### Pattern 2: Event-Based System

**How it works**:
- Server sends only events as they happen
- Events: "Enemy spawned at X", "Hero fired at angle Y", "Enemy died"
- Client rebuilds state from events

**Pros**:
- Very low bandwidth (only send what changed)
- Natural for action games
- Easy to record/replay
- Scalable to many entities

**Cons**:
- Complex state management
- Event ordering issues
- Packet loss breaks everything (requires reliability layer)
- Hard to debug (state is implicit)

**Best for**:
- Turn-based or slow-paced games
- Games with infrequent state changes
- When events are naturally discrete

**Example Bandwidth**:
```
Typical events:
- Hero moved: 28 bytes (id + position)
- Enemy spawned: 32 bytes (id + type + position)
- Shot fired: 20 bytes (id + direction)
- Enemy killed: 8 bytes (id)

Average: ~5 events/sec × 25 bytes = 125 bytes/sec per player
```

### Pattern 3: Hybrid - Snapshot + Delta Compression

**How it works**:
- Send full snapshot every N ticks (e.g., every 1 second)
- Send delta updates between snapshots (only changed entities)
- Client reconstructs full state from baseline + deltas

**Pros**:
- Bandwidth efficient
- Handles packet loss (recovers at next snapshot)
- Good balance of simplicity and efficiency
- Industry standard for AAA games

**Cons**:
- More complex implementation
- Requires delta compression algorithm
- Still sends redundant data

**Best for**:
- Large entity counts (100-1000+)
- Reliable-ish networks
- Production multiplayer games

### Pattern 4: Command Pattern + Client Prediction

**How it works**:
- Client sends inputs (commands) to server
- Server simulates and sends back authoritative state
- Client predicts locally for responsiveness
- Server corrects client if prediction was wrong

**Pros**:
- Extremely low bandwidth (only send inputs)
- Feels responsive (client prediction)
- Server authoritative (no cheating)
- Scalable

**Cons**:
- Complex to implement
- Prediction errors cause rubber-banding
- Requires rollback/reconciliation
- Hard to debug

**Best for**:
- Fast-paced competitive games
- Low-latency requirements
- High player counts

---

## 4. Recommended Architecture

### Hybrid Approach: Snapshot Interpolation + Event Stream + Commands

**For your arena shooter, I recommend a hybrid approach**:

#### Server → Client: Snapshot + Events
- **Full snapshot** every tick (30/sec) with delta compression
- **Event stream** for important one-time events (shot fired, enemy killed)
- Snapshots contain: entity positions, health, state
- Events contain: gameplay moments that need precise timing

#### Client → Server: Commands
- Client sends input commands (move direction, aim angle, fire)
- Lightweight (10-20 bytes per command)
- Server validates and simulates

#### Client Rendering: Interpolation + Event Playback
- Client interpolates entity positions between snapshots (smooth 60+ fps)
- Client plays events as they arrive (spawn particles, play sounds)
- Client runs "predicted" simulation for local player only

### Why This Approach?

✅ **Simple to Understand**: Clear separation of concerns
✅ **Easy to Debug**: Can inspect snapshots and events separately  
✅ **Performant**: Delta compression keeps bandwidth reasonable
✅ **Responsive**: Client prediction for local player
✅ **Robust**: Snapshots handle packet loss gracefully
✅ **Scalable**: Works for 10-100 entities easily

### Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                         SERVER                               │
│                                                              │
│  ┌─────────────────────────────────────────────────────┐   │
│  │         Simulation Core (Shared Assembly)            │   │
│  │  - Deterministic game logic (30 tick)               │   │
│  │  - Pure C# (no Unity dependencies)                  │   │
│  │  - Fixed-point math (Fix64)                         │   │
│  │  - Input → State → Events                           │   │
│  └─────────────────────────────────────────────────────┘   │
│                            ↕                                 │
│  ┌─────────────────────────────────────────────────────┐   │
│  │            Server Network Layer                      │   │
│  │  - Receive player commands                          │   │
│  │  - Send snapshots (delta compressed)                │   │
│  │  - Send events                                      │   │
│  │  - Validate inputs                                  │   │
│  └─────────────────────────────────────────────────────┘   │
└───────────────────────────┬─────────────────────────────────┘
                            │ Network (UDP/WebRTC)
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                         CLIENT                               │
│                                                              │
│  ┌─────────────────────────────────────────────────────┐   │
│  │         Simulation Core (Shared Assembly)            │   │
│  │  - Same code as server                              │   │
│  │  - Runs prediction for local player only            │   │
│  │  - Processes events                                 │   │
│  └─────────────────────────────────────────────────────┘   │
│                            ↕                                 │
│  ┌─────────────────────────────────────────────────────┐   │
│  │            Client Network Layer                      │   │
│  │  - Send player commands                             │   │
│  │  - Receive snapshots                                │   │
│  │  - Receive events                                   │   │
│  │  - Buffer and interpolate                           │   │
│  └─────────────────────────────────────────────────────┘   │
│                            ↕                                 │
│  ┌─────────────────────────────────────────────────────┐   │
│  │         Presentation Layer (Unity)                   │   │
│  │  - Interpolated rendering (60+ fps)                 │   │
│  │  - Visual effects (particles, sounds)               │   │
│  │  - UI updates                                       │   │
│  │  - Camera, input handling                           │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

---

## 5. Assembly Structure

### Folder Structure

```
Assets/
├── Shared/                          # NO UNITY DEPENDENCIES
│   ├── Shared.asmdef               # Assembly definition
│   ├── Core/
│   │   ├── SimulationWorld.cs      # Main simulation state
│   │   ├── SimulationTick.cs       # Tick counter, fixed delta
│   │   ├── ISimulationSystem.cs    # System interface
│   │   └── SimulationConfig.cs     # Constants
│   ├── Math/
│   │   ├── FixedMath.Net.cs        # Fixed-point library
│   │   ├── Fix64Vector2.cs         # 2D vector (Fix64)
│   │   ├── Fix64Vector3.cs         # 3D vector (Fix64)
│   │   └── FixedRandom.cs          # Deterministic random
│   ├── Entities/
│   │   ├── Entity.cs               # Base entity (id, position)
│   │   ├── Hero.cs                 # Hero state
│   │   ├── Enemy.cs                # Enemy state
│   │   └── Projectile.cs           # Projectile state
│   ├── Systems/
│   │   ├── MovementSystem.cs       # Entity movement
│   │   ├── CombatSystem.cs         # Damage, health
│   │   ├── SpawnSystem.cs          # Entity spawning
│   │   └── CollisionSystem.cs      # Collision detection
│   ├── Commands/
│   │   ├── ICommand.cs             # Command interface
│   │   ├── MoveCommand.cs          # Player movement input
│   │   └── ShootCommand.cs         # Player shoot input
│   ├── Events/
│   │   ├── ISimulationEvent.cs     # Event interface
│   │   ├── ShotFiredEvent.cs       # Gun fired
│   │   ├── EnemyKilledEvent.cs     # Enemy died
│   │   └── WaveStartedEvent.cs     # Wave begin
│   └── Data/
│       ├── LevelData.cs            # Level config (from JSON)
│       ├── WaveData.cs             # Wave config
│       └── EnemySpawnData.cs       # Enemy stats
│
├── Scripts/                         # UNITY CLIENT CODE
│   ├── Client.asmdef               # References Shared
│   ├── ClientSimulationDriver.cs   # Runs simulation, interpolates
│   ├── EntityView.cs               # Renders entities
│   ├── HeroView.cs                 # Hero rendering
│   ├── EnemyView.cs                # Enemy rendering
│   ├── ProjectileView.cs           # Projectile rendering
│   ├── InputManager.cs             # Convert Unity input → Commands
│   ├── NetworkClient.cs            # Network communication
│   └── GameUI.cs                   # UI updates
│
└── Server/                          # SERVER CODE (optional Unity)
    ├── Server.asmdef               # References Shared
    ├── ServerSimulationDriver.cs   # Runs authoritative simulation
    ├── NetworkServer.cs            # Network communication
    └── ServerMain.cs               # Entry point
```

### Shared.asmdef Configuration

```json
{
  "name": "Shared",
  "rootNamespace": "ArenaGame.Shared",
  "references": [],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": true
}
```

**Key**: `"noEngineReferences": true` prevents Unity types from being used!

---

## 6. State Synchronization Strategies

### Client State Machine

```
┌──────────────┐
│  CONNECTING  │ ← Initial state
└──────┬───────┘
       │ Connected
       ▼
┌──────────────┐
│   LOADING    │ ← Loading game data
└──────┬───────┘
       │ Ready
       ▼
┌──────────────┐
│   PLAYING    │ ← Active gameplay
└──────┬───────┘
       │ Interpolating snapshots
       │ Playing events
       │ Sending commands
       │
       │ Disconnected
       ▼
┌──────────────┐
│ DISCONNECTED │
└──────────────┘
```

### Snapshot Buffer

Client maintains a buffer of recent snapshots for interpolation:

```csharp
public class SnapshotBuffer
{
    private Queue<Snapshot> snapshots = new Queue<Snapshot>();
    private const int BufferSize = 3; // 100ms buffer at 30 tick
    
    public void AddSnapshot(Snapshot snapshot)
    {
        snapshots.Enqueue(snapshot);
        while (snapshots.Count > BufferSize)
            snapshots.Dequeue();
    }
    
    public Snapshot GetInterpolated(float renderTime)
    {
        // Find two snapshots to interpolate between
        // Return interpolated state
    }
}
```

**Benefits**:
- Smooth rendering even with jitter
- Handles out-of-order packets
- Adds ~100ms latency (acceptable for non-competitive game)

### Event Queue

Events are played back as they arrive:

```csharp
public class EventQueue
{
    private Queue<ISimulationEvent> events = new Queue<ISimulationEvent>();
    
    public void AddEvent(ISimulationEvent evt)
    {
        events.Enqueue(evt);
    }
    
    public void ProcessEvents(IEventHandler handler)
    {
        while (events.Count > 0)
        {
            var evt = events.Dequeue();
            handler.HandleEvent(evt);
        }
    }
}
```

### How Client Knows When Hero Fires

**Event-Driven Approach** (Recommended):

1. **Server-side**: Hero fires → `ShotFiredEvent` generated
2. **Network**: Event sent to all clients immediately
3. **Client**: Receives event → plays gun sound, spawns muzzle flash
4. **Result**: All clients see shot at same time (relative to their latency)

```csharp
// Shared: Event definition
public class ShotFiredEvent : ISimulationEvent
{
    public int EntityId;
    public Fix64Vector2 Position;
    public Fix64Vector2 Direction;
    public int TickNumber;
}

// Client: Event handler
public class HeroView : IEventHandler
{
    public void HandleEvent(ISimulationEvent evt)
    {
        if (evt is ShotFiredEvent shot)
        {
            // Play visual/audio effects
            PlayGunSound(shot.Position);
            SpawnMuzzleFlash(shot.Position, shot.Direction);
            SpawnBulletTracer(shot.Position, shot.Direction);
        }
    }
}
```

**Why not polling?**
- ❌ Wastes bandwidth checking every frame
- ❌ Misses events between frames
- ❌ Tight coupling between systems
- ❌ Harder to debug

**Why not just snapshots?**
- ❌ One-time events don't fit snapshot model
- ❌ Would need "dirty flags" on every entity
- ❌ Hard to sync timing (shot happened between snapshots)

**Event-driven wins because**:
- ✅ Events are naturally one-time occurrences
- ✅ Can be timestamped for precise playback
- ✅ Low bandwidth (only when something happens)
- ✅ Easy to record/replay
- ✅ Clear separation of concerns

---

## 7. Performance Considerations

### Bandwidth Optimization

#### Delta Compression
Only send changed entity data:

```csharp
public class DeltaCompressor
{
    private Snapshot lastSent;
    
    public byte[] Compress(Snapshot current)
    {
        var delta = new Delta();
        
        foreach (var entity in current.Entities)
        {
            var previous = lastSent.GetEntity(entity.Id);
            if (previous == null || entity.HasChanged(previous))
            {
                delta.Add(entity); // Only send if changed
            }
        }
        
        lastSent = current;
        return delta.Serialize();
    }
}
```

**Savings**: 50-80% bandwidth reduction

#### Quantization
Reduce precision for non-critical data:

```csharp
// Position: Full precision (24 bytes)
Fix64 x, y, z;

// Velocity: Half precision (12 bytes)
Fix32 vx, vy, vz;

// Health: Integer (2 bytes)
ushort health;
```

#### Interest Management
Only send entities near the player:

```csharp
public bool ShouldSyncToClient(Entity entity, Vector2 clientPos)
{
    float distance = Vector2.Distance(entity.Position, clientPos);
    return distance < VIEW_RADIUS;
}
```

**Savings**: 70-90% for large maps

### CPU Optimization

#### Spatial Partitioning
Use grid or quadtree for collision detection:

```
Grid Cell Size = 10 units
O(n²) → O(n) collision checks
```

#### Object Pooling
Reuse entity instances:

```csharp
public class EntityPool
{
    private Stack<Entity> pool = new Stack<Entity>();
    
    public Entity Spawn()
    {
        return pool.Count > 0 ? pool.Pop() : new Entity();
    }
    
    public void Despawn(Entity entity)
    {
        entity.Reset();
        pool.Push(entity);
    }
}
```

#### Batch Processing
Process entities in batches:

```csharp
// Instead of:
foreach (var entity in entities)
    entity.Update();

// Do:
MovementSystem.UpdateAll(entities); // Better cache locality
```

### Memory Optimization

#### Struct vs Class
Use structs for small, immutable data:

```csharp
// Good: Value type, no GC
public struct Fix64Vector2
{
    public Fix64 X;
    public Fix64 Y;
}

// Bad: Reference type, GC pressure
public class Vector2Data
{
    public Fix64 X;
    public Fix64 Y;
}
```

#### Array vs List
Pre-allocate arrays when size is known:

```csharp
// Good: No reallocations
Entity[] entities = new Entity[MAX_ENTITIES];

// Bad: Frequent reallocations
List<Entity> entities = new List<Entity>();
```

---

## 8. Implementation Roadmap

### Phase 1: Shared Assembly Setup (Week 1)
- [ ] Create `Assets/Shared/` folder
- [ ] Create `Shared.asmdef` with `noEngineReferences: true`
- [ ] Add FixedMath.Net library
- [ ] Create Fix64Vector2/3 wrappers
- [ ] Create basic entity structs (Hero, Enemy, Projectile)
- [ ] Test compilation in isolation

### Phase 2: Simulation Core (Week 2)
- [ ] Port game logic to shared assembly
- [ ] Create `SimulationWorld` class
- [ ] Implement fixed 30-tick loop
- [ ] Port hero movement (Fix64 math)
- [ ] Port enemy movement
- [ ] Port combat system (damage, health)
- [ ] Add deterministic random number generator
- [ ] Write unit tests

### Phase 3: Client Rendering Bridge (Week 3)
- [ ] Create `ClientSimulationDriver`
- [ ] Implement snapshot buffer (interpolation)
- [ ] Create `EntityView` base class
- [ ] Port `HeroView`, `EnemyView`, `ProjectileView`
- [ ] Connect Unity rendering to simulation state
- [ ] Test interpolation smoothness
- [ ] Add event system (shot fired, enemy killed)

### Phase 4: Event System (Week 4)
- [ ] Define `ISimulationEvent` interface
- [ ] Create event types (shot, kill, spawn, wave)
- [ ] Implement event queue
- [ ] Generate events from simulation
- [ ] Play events on client (VFX, SFX)
- [ ] Test event timing and synchronization

### Phase 5: Network Foundation (Week 5-6)
- [ ] Research Unity Netcode vs custom solution
- [ ] Implement basic client-server connection
- [ ] Send commands (client → server)
- [ ] Send snapshots (server → client)
- [ ] Send events (server → client)
- [ ] Test with localhost
- [ ] Add delta compression

### Phase 6: Server Build (Week 7)
- [ ] Create dedicated server project
- [ ] Remove Unity dependencies from server
- [ ] Headless mode configuration
- [ ] Multi-client testing
- [ ] Performance profiling

### Phase 7: Polish & Testing (Week 8+)
- [ ] Client prediction for local player
- [ ] Lag compensation
- [ ] Network condition simulation
- [ ] Stress testing (100+ entities)
- [ ] Bug fixes and optimization

---

## 9. Code Examples

### Example 1: Deterministic Hero

```csharp
// Shared/Entities/Hero.cs
using ArenaGame.Shared.Math;

namespace ArenaGame.Shared.Entities
{
    public struct Hero
    {
        public int Id;
        public Fix64Vector2 Position;
        public Fix64Vector2 Velocity;
        public Fix64 Health;
        public Fix64 MaxHealth;
        public Fix64 MoveSpeed;
        public Fix64 AimAngle;
        public int LastShotTick;
        public int ShotCooldownTicks; // 30 ticks = 1 second
        
        public bool CanShoot(int currentTick)
        {
            return currentTick - LastShotTick >= ShotCooldownTicks;
        }
        
        public void TakeDamage(Fix64 damage)
        {
            Health -= damage;
            if (Health < Fix64.Zero)
                Health = Fix64.Zero;
        }
        
        public bool IsAlive()
        {
            return Health > Fix64.Zero;
        }
    }
}
```

### Example 2: Movement System

```csharp
// Shared/Systems/MovementSystem.cs
using ArenaGame.Shared.Math;
using ArenaGame.Shared.Entities;

namespace ArenaGame.Shared.Systems
{
    public static class MovementSystem
    {
        public static void UpdateHero(ref Hero hero, Fix64Vector2 inputDir, Fix64 fixedDelta)
        {
            // Normalize input
            if (inputDir.Magnitude() > Fix64.One)
                inputDir = inputDir.Normalized();
            
            // Apply movement
            hero.Velocity = inputDir * hero.MoveSpeed;
            hero.Position += hero.Velocity * fixedDelta;
            
            // Clamp to arena bounds
            Fix64 maxRadius = Fix64.FromInt(10);
            if (hero.Position.Magnitude() > maxRadius)
            {
                hero.Position = hero.Position.Normalized() * maxRadius;
            }
        }
        
        public static void UpdateEnemy(ref Enemy enemy, Fix64Vector2 targetPos, Fix64 fixedDelta)
        {
            Fix64Vector2 dir = (targetPos - enemy.Position).Normalized();
            enemy.Velocity = dir * enemy.MoveSpeed;
            enemy.Position += enemy.Velocity * fixedDelta;
        }
    }
}
```

### Example 3: Simulation World

```csharp
// Shared/Core/SimulationWorld.cs
using System.Collections.Generic;
using ArenaGame.Shared.Events;
using ArenaGame.Shared.Commands;

namespace ArenaGame.Shared.Core
{
    public class SimulationWorld
    {
        public int CurrentTick { get; private set; }
        public Fix64 FixedDelta { get; private set; }
        
        private Dictionary<int, Hero> heroes = new Dictionary<int, Hero>();
        private Dictionary<int, Enemy> enemies = new Dictionary<int, Enemy>();
        private List<ISimulationEvent> eventBuffer = new List<ISimulationEvent>();
        
        public SimulationWorld()
        {
            FixedDelta = Fix64.FromFloat(1f / 30f); // 30 tick rate
        }
        
        public void Tick(List<ICommand> commands)
        {
            CurrentTick++;
            eventBuffer.Clear();
            
            // Process commands
            foreach (var cmd in commands)
                cmd.Execute(this);
            
            // Update systems
            MovementSystem.UpdateAll(heroes, enemies, FixedDelta);
            CombatSystem.UpdateAll(heroes, enemies, FixedDelta, eventBuffer);
            SpawnSystem.Update(this, CurrentTick);
            
            // Collision detection
            CollisionSystem.DetectAll(heroes, enemies, eventBuffer);
        }
        
        public List<ISimulationEvent> GetEvents()
        {
            return new List<ISimulationEvent>(eventBuffer);
        }
        
        public Snapshot CreateSnapshot()
        {
            return new Snapshot
            {
                Tick = CurrentTick,
                Heroes = new List<Hero>(heroes.Values),
                Enemies = new List<Enemy>(enemies.Values)
            };
        }
    }
}
```

### Example 4: Client Simulation Driver

```csharp
// Scripts/ClientSimulationDriver.cs
using UnityEngine;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Events;

namespace ArenaGame.Client
{
    public class ClientSimulationDriver : MonoBehaviour
    {
        private SimulationWorld simulation;
        private SnapshotBuffer snapshotBuffer;
        private EventQueue eventQueue;
        private float accumulator;
        
        void Start()
        {
            simulation = new SimulationWorld();
            snapshotBuffer = new SnapshotBuffer();
            eventQueue = new EventQueue();
        }
        
        void Update()
        {
            // Accumulate time for fixed ticks
            accumulator += Time.deltaTime;
            
            float tickTime = 1f / 30f;
            while (accumulator >= tickTime)
            {
                // Tick simulation (prediction only for local player)
                simulation.Tick(GetLocalCommands());
                accumulator -= tickTime;
            }
            
            // Get interpolated state for rendering
            float renderTime = Time.time - 0.1f; // 100ms behind for smoothness
            Snapshot interpolated = snapshotBuffer.GetInterpolated(renderTime);
            
            // Update entity views
            UpdateViews(interpolated);
            
            // Process events
            ProcessEvents();
        }
        
        public void OnSnapshotReceived(Snapshot snapshot)
        {
            snapshotBuffer.AddSnapshot(snapshot);
        }
        
        public void OnEventReceived(ISimulationEvent evt)
        {
            eventQueue.AddEvent(evt);
        }
        
        void ProcessEvents()
        {
            eventQueue.ProcessEvents((evt) => {
                // Find view and notify
                if (evt is ShotFiredEvent shot)
                {
                    var view = FindViewForEntity(shot.EntityId);
                    view?.OnShotFired(shot);
                }
            });
        }
    }
}
```

### Example 5: Entity View (Rendering)

```csharp
// Scripts/HeroView.cs
using UnityEngine;
using ArenaGame.Shared.Entities;
using ArenaGame.Shared.Events;
using ArenaGame.Shared.Math;

namespace ArenaGame.Client
{
    public class HeroView : MonoBehaviour, IEventHandler
    {
        public int EntityId;
        public GameObject muzzleFlashPrefab;
        public AudioClip gunShotSound;
        
        private Vector3 renderPosition;
        private Vector3 targetPosition;
        
        public void UpdateFromSnapshot(Hero hero)
        {
            // Convert Fix64 to float for Unity
            targetPosition = new Vector3(
                (float)hero.Position.X,
                0.5f,
                (float)hero.Position.Y
            );
            
            // Smooth interpolation
            renderPosition = Vector3.Lerp(renderPosition, targetPosition, 0.5f);
            transform.position = renderPosition;
            
            // Update rotation
            float angle = (float)hero.AimAngle;
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
        
        public void HandleEvent(ISimulationEvent evt)
        {
            if (evt is ShotFiredEvent shot && shot.EntityId == EntityId)
            {
                // Play effects
                if (muzzleFlashPrefab != null)
                {
                    var flash = Instantiate(muzzleFlashPrefab, transform.position, Quaternion.identity);
                    Destroy(flash, 0.1f);
                }
                
                if (gunShotSound != null)
                {
                    AudioSource.PlayClipAtPoint(gunShotSound, transform.position);
                }
            }
        }
    }
}
```

---

## 10. References and Resources

### Academic Papers
- **"Fast-Paced Multiplayer" by Gabriel Gambetta**
  - URL: https://www.gabrielgambetta.com/client-server-game-architecture.html
  - The bible of client-server game architecture

- **"Deterministic Lockstep" by Gaffer On Games**
  - URL: https://gafferongames.com/post/deterministic_lockstep/
  - Deep dive into deterministic simulation

- **"1500 Archers on a 28.8" by Age of Empires dev**
  - Classic RTS networking strategies

### Open Source Examples
- **Unity DOTS NetCode Samples**
  - GitHub: Unity-Technologies/EntityComponentSystemSamples
  - Production-quality examples

- **Photon BOLT Samples**
  - Client prediction and rollback examples

- **Mirror Networking**
  - GitHub: vis2k/Mirror
  - Popular open-source Unity networking

### Libraries
- **FixedMath.Net**
  - GitHub: asik/FixedMath.Net
  - Fixed-point math library

- **MessagePack-CSharp**
  - GitHub: neuecc/MessagePack-CSharp
  - Fast serialization for snapshots

- **LiteNetLib**
  - GitHub: RevenantX/LiteNetLib
  - Lightweight UDP networking

### Books
- **"Multiplayer Game Programming" by Joshua Glazer**
  - Comprehensive guide to multiplayer architecture

- **"Network Programming for Games" by Glenn Fiedler**
  - Low-level networking concepts

### Videos
- **GDC: "Overwatch Gameplay Architecture"**
  - Blizzard's approach to client-server

- **GDC: "I Shot You First: Networking the Gameplay of Halo: Reach"**
  - Bungie's networking strategies

### Tools
- **Wireshark** - Network packet analysis
- **Clumsy** - Network condition simulation (latency, packet loss)
- **Unity Profiler** - Performance analysis

---

## Summary

### Key Takeaways

1. **Use Fixed-Point Math** (FixedMath.Net) for determinism
2. **30 Tick Simulation** is optimal for your game type
3. **Hybrid Architecture**: Snapshots + Events + Commands
4. **Separate Rendering from Simulation** completely
5. **Event-Driven** for one-time occurrences (shot fired)
6. **Interpolation** for smooth 60+ fps rendering
7. **Shared Assembly** with zero Unity dependencies

### Architecture Decision Summary

| Aspect | Decision | Reason |
|--------|----------|--------|
| Math Library | FixedMath.Net | Deterministic, simple API, no dependencies |
| Tick Rate | 30 ticks/sec | Balance of bandwidth and responsiveness |
| Sync Pattern | Snapshot + Events | Simple, robust, debuggable |
| Client Render | Interpolation | Smooth visuals with 100ms delay |
| Hero Shooting | Event-driven | Natural for one-time actions |
| State Updates | Snapshot polling (30Hz) | Continuous entity state |
| Assembly | Shared with no Unity | Code reuse, enforced separation |

### Next Steps

1. **Review this document** with team
2. **Create Shared assembly** and test with simple entity
3. **Port one system** (e.g., hero movement) to prove concept
4. **Build prototype** with snapshot interpolation
5. **Add event system** for shooting
6. **Iterate** based on performance and feel

---

**Document Version**: 1.0  
**Last Updated**: October 15, 2025  
**Author**: Architecture Research Team  
**Status**: Ready for Implementation Planning

