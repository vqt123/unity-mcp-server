# Level and Wave System

## Overview

The game now features a comprehensive level and wave system that progressively increases difficulty. Each level consists of multiple waves, with the final wave being a boss encounter.

## Architecture

### Components

1. **ConfigManager.cs** - Handles loading and parsing level configuration
2. **LevelManager.cs** - Controls level progression, wave timing, and difficulty scaling
3. **EnemySpawner.cs** - Spawns enemies based on wave configuration
4. **Levels.json** - Configuration file defining all levels and waves

### Data Structures

#### LevelData
- `levelNumber`: Unique identifier for the level
- `levelName`: Display name (e.g., "The Beginning", "Apocalypse")
- `difficultyMultiplier`: Global multiplier applied to all enemy stats
- `waves`: Array of wave configurations

#### WaveData
- `waveNumber`: Wave identifier within the level
- `duration`: How long the wave lasts (seconds)
- `spawnInterval`: Time between enemy spawns (seconds)
- `enemies`: Array of enemy spawn configurations
- `isBossWave`: Boolean indicating if this is a boss wave
- `isMiniBossWave`: Boolean indicating if this is a mini-boss wave
- `bossType`: String identifier for the boss type (optional)

#### EnemySpawnData
- `enemyType`: Type of enemy (e.g., "basic", "miniboss", "boss")
- `count`: Number of this enemy type to spawn
- `health`: Health multiplier for this enemy
- `damage`: Damage multiplier for this enemy
- `moveSpeed`: Move speed multiplier for this enemy

## Level Progression

### Current Levels

**Level 1: The Beginning** (Difficulty: 1.0x)
- 4 waves
- Introduces basic enemies
- Final wave: Boss

**Level 2: Rising Threat** (Difficulty: 1.3x)
- 5 waves
- Increased enemy count and stats
- Wave 3: Mini-boss encounter
- Final wave: Enhanced boss

**Level 3: The Onslaught** (Difficulty: 1.6x)
- 5 waves
- Multiple mini-boss encounters
- Final wave: Elite boss with multiple enemies

**Level 4: Nightmare Realm** (Difficulty: 2.0x)
- 6 waves
- Heavy enemy pressure
- Multiple mini-boss waves
- Final wave: Nightmare Lord with support enemies

**Level 5: Apocalypse** (Difficulty: 2.5x)
- 5 waves
- Every wave has mini-bosses
- Two boss waves
- Final wave: Multiple bosses simultaneously

## Difficulty Scaling

### Level Multiplier
Each level has a global `difficultyMultiplier` that affects all enemies:
- Level 1: 1.0x (baseline)
- Level 2: 1.3x
- Level 3: 1.6x
- Level 4: 2.0x
- Level 5: 2.5x

### Wave Multipliers
Each enemy spawn within a wave has individual multipliers:
- `health`: Scales enemy health
- `damage`: Scales enemy damage
- `moveSpeed`: Scales enemy movement speed

### Boss Modifiers
Special modifiers are automatically applied:

**Boss Wave:**
- Health: 5x multiplier
- Damage: 2x multiplier
- Move Speed: 0.8x multiplier
- Visual: 2x scale, red color

**Mini-Boss Wave:**
- Health: 2.5x multiplier
- Damage: 1.5x multiplier
- Move Speed: varies
- Visual: 1.5x scale, yellow color

### Final Calculation
```
finalHealth = baseHealth × waveHealthMultiplier × levelDifficultyMultiplier × bossModifier
finalDamage = baseDamage × waveDamageMultiplier × levelDifficultyMultiplier × bossModifier
finalMoveSpeed = baseMoveSpeed × waveMoveSpeedMultiplier
```

## Wave Mechanics

### Wave Flow
1. **Wave Start**: LevelManager starts the wave, configures EnemySpawner
2. **Spawning**: Enemies spawn at intervals until wave duration ends
3. **Wave End**: After duration + all enemies spawned, 3-second delay
4. **Next Wave**: Automatically starts next wave or completes level

### Spawn Behavior
- Enemies spawn at the edge of the arena at random angles
- Spawn order is shuffled for variety
- Respects `maxEnemies` limit (default: 20 concurrent enemies)
- If max reached, spawning delays until enemies are killed

### Level Completion
When all waves in a level are complete:
- 5-second delay
- Automatically loads next level
- If no next level exists, game is complete

## UI Display

### Level Display
- **Position**: Top center of screen
- **Format**: "Level X: Level Name"
- **Color**: Yellow with black outline
- **Font Size**: 32

### Wave Display
- **Position**: Below level display
- **Format**: "Wave X/Y" or "Wave X/Y [BOSS]"
- **Color**: Cyan with black outline
- **Font Size**: 28
- **Special Indicators**: [BOSS], [MINI-BOSS]

## Configuration

### Adding a New Level

1. Open `Assets/Resources/Levels.json`
2. Add a new level object to the `levels` array:

```json
{
  "levelNumber": 6,
  "levelName": "Your Level Name",
  "difficultyMultiplier": 3.0,
  "waves": [
    {
      "waveNumber": 1,
      "duration": 60,
      "spawnInterval": 2.0,
      "isBossWave": false,
      "isMiniBossWave": false,
      "enemies": [
        {
          "enemyType": "basic",
          "count": 50,
          "health": 5.0,
          "damage": 4.0,
          "moveSpeed": 2.5
        }
      ]
    }
  ]
}
```

### Design Guidelines

**Easy Waves:**
- Duration: 30-40 seconds
- Spawn Interval: 2.5-3.0 seconds
- Enemy Count: 10-20
- Stat Multipliers: 1.0-1.5x

**Medium Waves:**
- Duration: 40-60 seconds
- Spawn Interval: 1.5-2.5 seconds
- Enemy Count: 20-35
- Stat Multipliers: 1.5-2.5x

**Hard Waves:**
- Duration: 60-90 seconds
- Spawn Interval: 1.0-2.0 seconds
- Enemy Count: 35-50
- Stat Multipliers: 2.5-4.0x

**Boss Waves:**
- Duration: 60-120 seconds
- Spawn Interval: 2.0-3.0 seconds
- Boss Count: 1-5
- Support Enemies: 10-40
- Boss multipliers applied automatically

## Integration with Existing Systems

### XP and Leveling
- Players still gain XP from kills
- Level ups still trigger upgrade selection
- Hero progression independent of game levels

### Enemy Behavior
- All enemies use the same AI
- Stats are scaled based on wave/level configuration
- Visual changes (size, color) for bosses

### Game Manager
- GameManager and LevelManager run independently
- EnemySpawner controlled by LevelManager
- Kill tracking still in GameManager

## Testing

### Manual Testing
1. Start ArenaGame scene
2. Select a hero
3. Observe wave progression
4. Check enemy scaling
5. Verify UI updates
6. Test boss waves

### Debug Commands
```csharp
// In LevelManager.cs
Debug.Log($"[LevelManager] Starting Wave {currentWaveNumber + 1}/{currentLevel.waves.Count}");
Debug.Log($"[LevelManager] - Duration: {currentWave.duration}s");
Debug.Log($"[LevelManager] - Total Enemies: {totalEnemiesToSpawn}");
```

### Common Issues

**Enemies not spawning:**
- Check Levels.json syntax
- Verify ConfigManager loaded levels
- Check EnemySpawner has LevelManager reference

**Waves not progressing:**
- Ensure wave duration is reasonable
- Check all enemies were spawned
- Verify Update() is being called

**Difficulty too easy/hard:**
- Adjust level difficultyMultiplier
- Modify wave enemy multipliers
- Change spawn intervals

## Future Enhancements

### Planned Features
- [ ] Different enemy types (ranged, tank, fast)
- [ ] Wave modifiers (double speed, extra health)
- [ ] Special events (meteor shower, healing zones)
- [ ] Dynamic difficulty based on player performance
- [ ] Endless mode with infinite scaling
- [ ] Boss introduction cutscenes
- [ ] Wave cleared rewards (bonus XP, items)

### Enemy Variety
Currently all enemies use the same prefab with scaled stats. Future improvements:
- Unique boss prefabs with special abilities
- Enemy variants with different behaviors
- Environmental hazards

### Player Feedback
- Wave start/end announcements
- Boss entrance effects
- Victory celebrations
- Difficulty tier indicators

## Performance Considerations

- Max 20 concurrent enemies prevents performance issues
- Enemies are destroyed on death (no pooling yet)
- Wave configuration loaded once at level start
- Spawn calculations are lightweight

## Code Examples

### Starting a Specific Level
```csharp
LevelManager levelManager = FindFirstObjectByType<LevelManager>();
levelManager.LoadLevel(3); // Start at level 3
```

### Getting Current Difficulty
```csharp
float difficulty = levelManager.GetDifficultyMultiplier();
Debug.Log($"Current difficulty: {difficulty}x");
```

### Checking Wave Status
```csharp
if (levelManager.IsWaveActive())
{
    WaveData currentWave = levelManager.GetCurrentWave();
    Debug.Log($"Current wave: {currentWave.waveNumber}");
}
```

## Summary

The level and wave system provides:
- ✅ Progressive difficulty scaling
- ✅ Multiple enemy types (basic, mini-boss, boss)
- ✅ Visual feedback (UI, enemy colors/sizes)
- ✅ Flexible configuration via JSON
- ✅ 5 complete levels with 21 total waves
- ✅ Smooth progression and pacing
- ✅ Integration with existing systems

The system is designed to be easily expandable and configurable, allowing for rapid iteration and balancing.

