# Interpolation Learnings - Client-Server Smooth Movement

**Date**: October 2025  
**Status**: ✅ Working Implementation  
**Location**: `Assets/Scripts/Client/EntityVisualizer.cs`

---

## Problem Statement

Fixed timestep simulation runs at 10 ticks per second (100ms per tick), but Unity renders at 60+ fps. Without interpolation, entities appear to "stutter" or "skip" as they snap to each tick's position, creating choppy, non-smooth movement.

---

## Key Learnings

### 1. **You Need TWO Position Buffers (Not One)**

**Critical Insight**: For smooth interpolation, you must maintain **explicit positions from TWO ticks**:
- `previousTickPos`: Position from tick N-1
- `currentTickPos`: Position from tick N

**Why**: If you only store one position and update it when a new tick arrives, you immediately lose the position from tick N-1, making interpolation impossible within tick N.

### 2. **Buffer Update Timing is Critical**

**Correct Pattern**:
1. When tick N+1 arrives:
   - `previousTickPos` ← `currentTickPos` (move tick N → previous)
   - `currentTickPos` ← simulation position at tick N+1
2. For **all frames** in tick N+1:
   - Interpolate between `previousTickPos` (tick N) and `currentTickPos` (tick N+1)

**Wrong Pattern** (what we tried first):
- Updating stored position immediately when tick changes
- This causes distance to be 0 for most frames, creating visible stutter

### 3. **Interpolation Factor Calculation**

```csharp
float tickInterval = 1f / ticksPerSecond;  // e.g., 0.1s for 10 tps
float tickAccumulator += Time.deltaTime;    // Accumulates time since last tick

// Interpolation factor: 0.0 = at previous tick, 1.0 = at current tick
float interpolationFactor = Mathf.Clamp01(tickAccumulator / tickInterval);
```

**Important**: Use `Time.deltaTime` (not `Time.unscaledDeltaTime`) so interpolation responds to `Time.timeScale` changes.

### 4. **Data Structure**

The working implementation uses a `PositionBuffer` struct with:
```csharp
private struct PositionBuffer
{
    public Vector3 previousTickPos;  // Position from tick N-1
    public Vector3 currentTickPos;   // Position from tick N
    public int previousTick;         // Tick number for previousTickPos
    public int currentTick;          // Tick number for currentTickPos
}
```

This makes it explicit which positions belong to which ticks.

---

## Implementation Pattern

### Buffer Initialization

When an entity spawns:
```csharp
entityPositionBuffers[entityId] = new PositionBuffer
{
    previousTickPos = initialPos,
    currentTickPos = initialPos,
    previousTick = currentTick,
    currentTick = currentTick
};
```

### Buffer Update Logic

```csharp
if (currentTick > buffer.currentTick)
{
    // New tick! Shift positions
    buffer.previousTickPos = buffer.currentTickPos;  // N → N-1
    buffer.previousTick = buffer.currentTick;
    buffer.currentTickPos = currentSimPos;           // N+1 → N
    buffer.currentTick = currentTick;
}
else if (currentTick == buffer.currentTick)
{
    // Same tick, just update current position (simulation may have refined it)
    buffer.currentTickPos = currentSimPos;
}
```

### Interpolation

```csharp
// Always interpolate between previous (N-1) and current (N)
Vector3 interpolatedPos = Vector3.Lerp(
    buffer.previousTickPos, 
    buffer.currentTickPos, 
    interpolationFactor
);
obj.transform.position = interpolatedPos;
```

---

## Common Pitfalls

### ❌ Pitfall 1: Single Position Storage

**Problem**: Storing only one position and updating it immediately when tick changes.

**Symptom**: Distance between previous and current is 0 for most frames, causing stutter.

**Fix**: Use explicit two-position buffer.

### ❌ Pitfall 2: Updating Buffer Too Early

**Problem**: Updating stored position in the first frame of a new tick, before all frames use it.

**Symptom**: Interpolation works for first frame, then breaks for rest of tick.

**Fix**: Only update buffer when tick actually changes, not within the tick.

### ❌ Pitfall 3: Interpolation Factor Calculation Errors

**Problem**: Calculating interpolation factor incorrectly or using wrong time source.

**Symptoms**: Objects appear to lag behind or jitter.

**Fix**: Ensure factor = `tickAccumulator / tickInterval`, clamped to [0, 1].

---

## Performance Considerations

- **Memory**: Two Vector3 positions per entity (24 bytes × 2 = 48 bytes per entity)
- **CPU**: One `Vector3.Lerp` per entity per frame (~3 float operations)
- **Scalability**: Works well for 100+ entities at 60+ fps

---

## Testing Checklist

✅ Projectiles move smoothly at high speeds  
✅ Movement remains smooth when `Time.timeScale` is changed  
✅ No visible stutter or skipping  
✅ Entities interpolate correctly even when simulation runs slower than render rate  
✅ Works for heroes, enemies, and projectiles consistently  

---

## Related Documentation

- `Main.md` - Overall project architecture
- `docs/archive/Client-Server-Architecture-Research.md` - Architecture research
- `Assets/Scripts/Client/EntityVisualizer.cs` - Implementation

---

## Code References

**Main Implementation**:
- `EntityVisualizer.SyncPositions()` - Lines 304-479
- `PositionBuffer` struct - Lines 28-34
- `entityPositionBuffers` dictionary - Line 36

**Simulation Timing**:
- `GameSimulation.TickAccumulator` - Exposes accumulator for interpolation
- `GameSimulation.TickInterval` - Exposes interval for interpolation

---

## Future Improvements

1. **Extrapolation**: Add extrapolation when tick accumulator exceeds 1.0 (rare, but possible with frame drops)
2. **Lag Compensation**: For multiplayer, store additional history for lag compensation
3. **Rotation Interpolation**: Extend to interpolate rotation/heading smoothly
4. **Jitter Detection**: Add metrics to detect when interpolation is struggling

---

**Lessons Learned**: The key was realizing that interpolation requires **two explicit positions**, not a single "previous" position that gets overwritten. The buffer pattern ensures we always have valid positions from tick N-1 and tick N to interpolate between.

