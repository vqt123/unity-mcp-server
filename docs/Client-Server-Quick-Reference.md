# Client-Server Architecture - Quick Reference

> **TL;DR**: Use shared assembly with FixedMath.Net, 30-tick simulation, snapshot interpolation + event stream for rendering. Event-driven for one-time actions like shooting.

---

## Decision Matrix

| Question | Answer | Why |
|----------|--------|-----|
| **Fixed-point library?** | FixedMath.Net | Deterministic, simple, no Unity deps |
| **Tick rate?** | 30 ticks/sec (33.33ms) | Optimal bandwidth vs responsiveness |
| **How does client know hero fired?** | Event sent from server | Low bandwidth, precise timing |
| **Polling vs events?** | Events for actions, snapshots for state | Best of both worlds |
| **Client rendering?** | Interpolate between snapshots | Smooth 60+ fps from 30 tick data |
| **Assembly structure?** | `Shared/` folder, no Unity refs | Code reuse, enforced separation |

---

## Architecture in One Diagram

```
SERVER (Authoritative)                CLIENT (Visual)
━━━━━━━━━━━━━━━━━━━━              ━━━━━━━━━━━━━━━━━━━━
┌─────────────────┐                ┌─────────────────┐
│  Shared Logic   │                │  Shared Logic   │
│  (30 tick/sec)  │                │  (prediction)   │
│  • Movement     │                │  • Same code    │
│  • Combat       │                │  • Local only   │
│  • Spawning     │                │                 │
└────────┬────────┘                └────────┬────────┘
         │                                  │
         │ Generate                         │ Consume
         ▼                                  ▼
┌─────────────────┐                ┌─────────────────┐
│  State + Events │───────────────>│ Snapshot Buffer │
│  • Snapshots    │   Network      │ • 3 snapshots   │
│  • Events       │   (UDP/WS)     │ • Interpolate   │
└─────────────────┘                └────────┬────────┘
                                            │
                                            ▼
                                   ┌─────────────────┐
                                   │ Unity Rendering │
                                   │ • 60+ fps       │
                                   │ • VFX/SFX       │
                                   │ • Smooth        │
                                   └─────────────────┘
```

---

## When Hero Fires - Sequence Diagram

```
Player          Client          Server          All Clients
  │               │               │                 │
  │ Press Fire    │               │                 │
  ├──────────────>│               │                 │
  │               │ Command       │                 │
  │               ├──────────────>│                 │
  │               │               │ Validate        │
  │               │               │ Simulate        │
  │               │               │ Create bullet   │
  │               │               │ ShotFiredEvent  │
  │               │<──────────────┤                 │
  │               │               ├────────────────>│
  │               │ Play VFX      │                 │
  │               │ Play SFX      │                 │
  │<──Visual──────┤               │                 │
  │               │               │                 │
  │               │ Snapshot      │                 │
  │               │<──────────────┤                 │
  │               │ (bullet pos)  │                 │
```

**Why this way?**
- ✅ Server is authoritative (no cheating)
- ✅ Events are immediate (responsive feel)
- ✅ Snapshots are continuous (smooth interpolation)
- ✅ Low bandwidth (events only when action happens)

---

## Folder Structure

```
Assets/
├── Shared/                    ← NO UNITY TYPES ALLOWED
│   ├── Shared.asmdef         ← "noEngineReferences": true
│   ├── Math/
│   │   └── FixedMath.Net.cs  ← Deterministic math
│   ├── Entities/
│   │   ├── Hero.cs           ← Pure data (Fix64)
│   │   └── Enemy.cs
│   ├── Systems/
│   │   ├── MovementSystem.cs ← Pure logic
│   │   └── CombatSystem.cs
│   └── Events/
│       ├── ShotFiredEvent.cs
│       └── EnemyKilledEvent.cs
│
└── Scripts/                   ← Unity client code
    ├── Client.asmdef
    ├── ClientSimulationDriver.cs
    ├── HeroView.cs            ← Renders Hero
    └── NetworkClient.cs
```

---

## Code Snippets

### 1. Deterministic Entity (Shared/)

```csharp
// Shared/Entities/Hero.cs
using ArenaGame.Shared.Math;

public struct Hero  // Struct = value type = no GC
{
    public int Id;
    public Fix64Vector2 Position;  // NOT UnityEngine.Vector2!
    public Fix64 Health;
    public Fix64 AimAngle;
    public int LastShotTick;
}
```

### 2. Simulation Tick (Shared/)

```csharp
// Shared/Core/SimulationWorld.cs
public class SimulationWorld
{
    public int CurrentTick { get; private set; }
    public Fix64 FixedDelta = Fix64.FromFloat(1f / 30f); // 33.33ms
    
    public void Tick(List<ICommand> commands)
    {
        CurrentTick++;
        
        // 1. Process commands
        foreach (var cmd in commands)
            cmd.Execute(this);
        
        // 2. Update systems (deterministic)
        MovementSystem.Update(heroes, FixedDelta);
        CombatSystem.Update(heroes, enemies, eventBuffer);
        
        // 3. Generate events
        // (added to eventBuffer during system updates)
    }
}
```

### 3. Client Rendering (Unity)

```csharp
// Scripts/ClientSimulationDriver.cs
public class ClientSimulationDriver : MonoBehaviour
{
    private SnapshotBuffer snapshots;
    private EventQueue events;
    
    void Update()
    {
        // Get interpolated state (100ms behind for smoothness)
        float renderTime = Time.time - 0.1f;
        Snapshot state = snapshots.GetInterpolated(renderTime);
        
        // Update all entity views
        foreach (var hero in state.Heroes)
        {
            var view = GetView(hero.Id);
            view.UpdatePosition(ToUnityVector(hero.Position));
        }
        
        // Process events (VFX, SFX)
        events.ProcessAll((evt) => {
            if (evt is ShotFiredEvent shot)
                PlayGunEffects(shot);
        });
    }
}
```

### 4. Event Handler (Unity)

```csharp
// Scripts/HeroView.cs
public class HeroView : MonoBehaviour
{
    public void OnShotFired(ShotFiredEvent evt)
    {
        // Visual
        Instantiate(muzzleFlashPrefab, transform.position, Quaternion.identity);
        
        // Audio
        AudioSource.PlayClipAtPoint(gunShotSound, transform.position);
        
        // Particles
        bulletTracer.Play();
    }
}
```

---

## Bandwidth Estimate

### Snapshot (uncompressed)
```
Hero: 64 bytes (pos, vel, health, etc.)
Enemy: 64 bytes
Projectile: 32 bytes

20 entities × 64 bytes × 30 ticks/sec = 38 KB/sec
```

### Delta Compressed
```
Only send changed entities = ~15 KB/sec (60% savings)
```

### Events
```
ShotFiredEvent: 20 bytes
EnemyKilledEvent: 8 bytes
Average: ~5 events/sec × 20 bytes = 100 bytes/sec
```

### Total: ~15-20 KB/sec per player

---

## Performance Targets

| Metric | Target | Why |
|--------|--------|-----|
| Tick Rate | 30 ticks/sec | Balance bandwidth/CPU |
| Render FPS | 60+ fps | Smooth visuals |
| Max Entities | 100 | Arena size limit |
| Bandwidth | <20 KB/sec/player | Mobile-friendly |
| Server CPU | <10ms per tick | Support many rooms |
| Memory | <100 MB | Shared hosting |

---

## Common Pitfalls

### ❌ DON'T: Use Unity types in Shared/
```csharp
// Shared/Entities/Hero.cs
public Vector3 Position; // ❌ UnityEngine.Vector3
```

### ✅ DO: Use custom types
```csharp
// Shared/Entities/Hero.cs
public Fix64Vector2 Position; // ✅ Deterministic
```

---

### ❌ DON'T: Poll for events every frame
```csharp
void Update() {
    if (hero.justFired) { // ❌ Wastes bandwidth
        PlayEffects();
    }
}
```

### ✅ DO: Use event callbacks
```csharp
void OnEvent(ShotFiredEvent evt) { // ✅ Efficient
    PlayEffects();
}
```

---

### ❌ DON'T: Use Time.deltaTime in simulation
```csharp
position += velocity * Time.deltaTime; // ❌ Non-deterministic
```

### ✅ DO: Use fixed delta
```csharp
position += velocity * FixedDelta; // ✅ Deterministic (33.33ms)
```

---

## Implementation Checklist

### Phase 1: Shared Assembly
- [ ] Create `Assets/Shared/` folder
- [ ] Create `Shared.asmdef` with `noEngineReferences: true`
- [ ] Add FixedMath.Net (single file, ~500 lines)
- [ ] Create Fix64Vector2/3 structs
- [ ] Test: Compile without Unity references

### Phase 2: Port One System
- [ ] Port Hero entity to Fix64
- [ ] Port MovementSystem
- [ ] Create SimulationWorld
- [ ] Run 30-tick loop
- [ ] Test: Same input = same output

### Phase 3: Client Rendering
- [ ] Create ClientSimulationDriver
- [ ] Implement snapshot buffer (3 snapshots)
- [ ] Create HeroView (interpolated rendering)
- [ ] Test: Smooth 60fps from 30 tick data

### Phase 4: Events
- [ ] Create ShotFiredEvent
- [ ] Generate event in CombatSystem
- [ ] Create EventQueue
- [ ] Handle event in HeroView (VFX/SFX)
- [ ] Test: Effects play immediately

---

## Questions & Answers

**Q: Why not just use Unity Netcode?**  
A: We want full control for custom server logic, determinism, and ability to run server outside Unity.

**Q: Why 30 tick instead of 60?**  
A: Halves bandwidth/CPU with imperceptible difference when interpolated.

**Q: Why events AND snapshots?**  
A: Snapshots for continuous state (position), events for discrete actions (shot fired).

**Q: Can we use Unity's Random?**  
A: No! Must use seeded deterministic random in Shared assembly.

**Q: What about Unity Physics?**  
A: Replace with custom collision detection using Fix64 math.

**Q: How to handle lag?**  
A: Client interpolates (100ms buffer), server is authoritative.

---

## Next Steps

1. **Read full architecture doc** (`Client-Server-Architecture-Research.md`)
2. **Set up Shared assembly** and verify no Unity refs
3. **Port Hero movement** as proof-of-concept
4. **Build minimal prototype** with interpolation
5. **Add shooting event** to validate event system
6. **Iterate and refine** based on feel

---

**See Also**:
- `Client-Server-Architecture-Research.md` - Full technical details
- `Level-Wave-System.md` - Current game systems
- Gabriel Gambetta's "Fast-Paced Multiplayer" articles

**Status**: Ready for Phase 1 implementation

