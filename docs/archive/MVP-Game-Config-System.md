# MVP Arena Game - JSON Config System

## Overview

The game uses **Newtonsoft.Json** to load hero and weapon configurations from JSON files, enabling data-driven game design without recompiling code.

---

## Architecture

### Config Files (Assets/Resources/)

1. **HeroTypes.json** - Defines 3 hero types with base stats
2. **WeaponTypes.json** - Defines 3 weapon types with combat stats

### Config Manager (Singleton)

**ConfigManager.cs** loads and provides access to config data:
- Loads JSON on startup
- Provides lookup methods: `GetHeroData()`, `GetWeaponData()`
- Provides list methods: `GetAllHeroes()`, `GetAllWeapons()`

### Hero System

**Hero.cs** initializes from both hero AND weapon configs:
- Loads hero base stats (health, color) from HeroTypes.json
- Loads weapon stats (damage, speed, cooldown, bullet color) from WeaponTypes.json
- Applies weapon stats to bullets when shooting

---

## Hero Types Configuration

**File:** `Assets/Resources/HeroTypes.json`

```json
{
  "heroes": [
    {
      "type": "Archer",
      "maxHealth": 80,
      "startingWeapon": "Bow",
      "color": { "r": 0.0, "g": 1.0, "b": 0.0, "a": 1.0 }
    },
    {
      "type": "Mage",
      "maxHealth": 60,
      "startingWeapon": "Fireball Staff",
      "color": { "r": 0.5, "g": 0.0, "b": 1.0, "a": 1.0 }
    },
    {
      "type": "Warrior",
      "maxHealth": 120,
      "startingWeapon": "Sword",
      "color": { "r": 1.0, "g": 0.0, "b": 0.0, "a": 1.0 }
    }
  ]
}
```

**Hero Properties:**
- `type` - Hero name/identifier
- `maxHealth` - Starting/max health points
- `startingWeapon` - Name of weapon from WeaponTypes.json
- `color` - RGB color for hero cube

---

## Weapon Types Configuration

**File:** `Assets/Resources/WeaponTypes.json`

```json
{
  "weapons": [
    {
      "name": "Bow",
      "damage": 15,
      "shootCooldown": 0.8,
      "bulletSpeed": 8,
      "bulletColor": { "r": 0.8, "g": 0.6, "b": 0.2, "a": 1.0 },
      "description": "Fast and accurate ranged weapon"
    },
    {
      "name": "Fireball Staff",
      "damage": 25,
      "shootCooldown": 1.5,
      "bulletSpeed": 6,
      "bulletColor": { "r": 1.0, "g": 0.3, "b": 0.0, "a": 1.0 },
      "description": "Powerful magical projectiles"
    },
    {
      "name": "Sword",
      "damage": 20,
      "shootCooldown": 1.2,
      "bulletSpeed": 5,
      "bulletColor": { "r": 0.7, "g": 0.7, "b": 0.7, "a": 1.0 },
      "description": "Balanced melee-range weapon"
    }
  ]
}
```

**Weapon Properties:**
- `name` - Weapon identifier (must match hero's startingWeapon)
- `damage` - Damage per bullet hit
- `shootCooldown` - Seconds between shots
- `bulletSpeed` - Bullet travel speed (units/sec)
- `bulletColor` - RGB color for bullets
- `description` - Human-readable description

---

## How It Works

### 1. Game Initialization

```
Main.cs Start()
  ↓
ConfigManager.Instance.LoadConfigs()
  ↓
Loads HeroTypes.json & WeaponTypes.json
  ↓
SpawnHeroes() - Creates all 3 heroes
```

### 2. Hero Initialization

```
Hero.Initialize(heroType)
  ↓
Load HeroData from config
  - maxHealth
  - startingWeapon name
  - hero color
  ↓
Load WeaponData from config
  - weaponDamage
  - weaponShootCooldown
  - weaponBulletSpeed
  - weaponBulletColor
  ↓
Hero is ready to fight!
```

### 3. Combat Loop

```
Hero.Update()
  ↓
Check if enough time passed (weaponShootCooldown)
  ↓
ShootAtClosestEnemy()
  ↓
Create bullet with:
  - Weapon damage
  - Weapon bullet speed
  - Weapon bullet color
  ↓
Bullet flies and hits enemy
```

---

## Weapon Balance

### Archer (Bow)
- **Role:** Fast DPS
- **Damage:** 15 (Low)
- **Cooldown:** 0.8s (Fast)
- **Speed:** 8 (Fast)
- **DPS:** 15 / 0.8 = **18.75 DPS**
- **Strategy:** Rapid fire, kiting

### Mage (Fireball Staff)
- **Role:** Burst Damage
- **Damage:** 25 (High)
- **Cooldown:** 1.5s (Slow)
- **Speed:** 6 (Medium)
- **DPS:** 25 / 1.5 = **16.67 DPS**
- **Strategy:** High-impact single shots

### Warrior (Sword)
- **Role:** Balanced Fighter
- **Damage:** 20 (Medium)
- **Cooldown:** 1.2s (Medium)
- **Speed:** 5 (Slow)
- **DPS:** 20 / 1.2 = **16.67 DPS**
- **Strategy:** Consistent damage, close-range

---

## Visual Identification

Each hero and their bullets have distinct colors:

| Hero | Hero Color | Weapon | Bullet Color |
|------|-----------|---------|--------------|
| Archer | Green | Bow | Gold/Tan |
| Mage | Purple | Fireball Staff | Orange/Red |
| Warrior | Red | Sword | Gray/Silver |

---

## Extending the System

### Adding a New Hero

1. Edit `HeroTypes.json`:
```json
{
  "type": "Rogue",
  "maxHealth": 70,
  "startingWeapon": "Dagger",
  "color": { "r": 0.0, "g": 0.0, "b": 1.0, "a": 1.0 }
}
```

2. Add corresponding weapon (or reuse existing)
3. Hero automatically spawns via `Main.cs`

### Adding a New Weapon

1. Edit `WeaponTypes.json`:
```json
{
  "name": "Dagger",
  "damage": 12,
  "shootCooldown": 0.5,
  "bulletSpeed": 10,
  "bulletColor": { "r": 0.3, "g": 0.3, "b": 0.3, "a": 1.0 },
  "description": "Ultra-fast assassin weapon"
}
```

2. Assign to hero's `startingWeapon`
3. Weapon stats automatically apply!

### Tweaking Balance

**No code changes needed!** Just edit JSON:

```bash
# Make Archer shoot faster
"shootCooldown": 0.8  →  0.6

# Make Mage fireballs stronger
"damage": 25  →  30

# Make Warrior tank more
"maxHealth": 120  →  150
```

Save JSON → Unity reloads → Test immediately!

---

## Implementation Details

### Why [System.NonSerialized]?

All hero/weapon fields use `[System.NonSerialized]` to prevent Unity from caching values in Inspector/prefabs:

```csharp
[System.NonSerialized]
public float weaponDamage;
```

**Benefits:**
- JSON configs are single source of truth
- No stale cached values
- No Inspector conflicts
- Clean prefabs

### Why Resources.Load()?

Bullet prefabs are loaded at runtime from `Resources/` folder:

```csharp
bulletPrefab = Resources.Load<GameObject>("Bullet");
```

**Benefits:**
- No Inspector assignments needed
- Works with [System.NonSerialized]
- Prefab updates immediately reflected
- No circular serialization issues

---

## File Structure

```
Assets/
├── Resources/
│   ├── HeroTypes.json       # 3 hero configs
│   ├── WeaponTypes.json     # 3 weapon configs
│   ├── Bullet.prefab        # Shared bullet prefab
│   └── Enemy.prefab         # Enemy prefab
├── Scripts/
│   ├── ConfigManager.cs     # JSON loader (singleton)
│   ├── Hero.cs              # Uses both hero + weapon configs
│   ├── Bullet.cs            # Receives weapon stats
│   ├── Enemy.cs             # Target for heroes
│   ├── EnemySpawner.cs      # Spawns enemies
│   ├── GameManager.cs       # UI and score
│   └── Main.cs              # Spawns all 3 heroes
├── Scenes/
│   └── ArenaGame.unity      # Main game scene
└── Main.cs                  # Scene root (spawns heroes)
```

---

## Data Flow Diagram

```
┌──────────────────────┐
│  HeroTypes.json      │
│  - Archer            │
│  - Mage              │
│  - Warrior           │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐     ┌──────────────────────┐
│  ConfigManager       │     │  WeaponTypes.json    │
│  (Singleton)         │◄────│  - Bow               │
│  - GetHeroData()     │     │  - Fireball Staff    │
│  - GetWeaponData()   │     │  - Sword             │
└──────────┬───────────┘     └──────────────────────┘
           │
           ▼
┌──────────────────────┐
│  Main.cs             │
│  SpawnHeroes()       │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│  Hero.Initialize()   │
│  - Load hero stats   │
│  - Load weapon stats │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│  Hero.ShootEnemy()   │
│  - Use weapon damage │
│  - Use weapon speed  │
│  - Use weapon color  │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│  Bullet              │
│  (Flies with weapon  │
│   stats)             │
└──────────────────────┘
```

---

## Testing the System

### 1. Play Mode Test

1. Enter Play mode in Unity
2. Observe 3 heroes spawn in center (Archer, Mage, Warrior)
3. Watch each hero shoot with different:
   - Fire rates (Archer fastest)
   - Bullet colors (Gold, Orange, Gray)
   - Damage values (Mage highest)

### 2. Config Changes Test

1. Edit `WeaponTypes.json`
2. Change Archer's `shootCooldown` to `0.3`
3. Re-enter Play mode
4. Archer now shoots 2.6x faster!

### 3. Balance Test

Watch the kill count and see which hero is most effective:
- Archer: High DPS, low per-hit
- Mage: Burst damage, slower
- Warrior: Balanced

---

## Key Takeaways

1. ✅ **Data-Driven Design** - All stats in JSON, not hardcoded
2. ✅ **3 Hero Types** - Archer, Mage, Warrior with unique stats
3. ✅ **3 Weapon Types** - Bow, Fireball Staff, Sword with unique behavior
4. ✅ **Newtonsoft.Json** - Industry-standard JSON parsing
5. ✅ **Single Source of Truth** - JSON configs (not Inspector)
6. ✅ **Easy Balancing** - Edit JSON → Test immediately
7. ✅ **Extensible** - Add heroes/weapons without code changes
8. ✅ **Visual Distinction** - Each hero/weapon has unique colors

---

## Next Steps (Optional)

### Add More Features

1. **Weapon Switching** - Heroes change weapons mid-game
2. **Weapon Pickup** - Enemies drop weapon powerups
3. **Weapon Upgrades** - Improve weapon stats over time
4. **Enemy Types** - Fast/slow/armored enemies from JSON
5. **Wave System** - Configure enemy waves in JSON
6. **Hero Abilities** - Special attacks from JSON config

### Improve Balance

1. Playtest and track which hero wins most
2. Adjust JSON values to balance DPS
3. Add unique mechanics per weapon type
4. Create rock-paper-scissors weapon matchups

### Polish

1. Add weapon-specific visual effects
2. Add weapon sound effects
3. Add weapon descriptions to UI
4. Show weapon stats on HUD
5. Add weapon upgrade trees

---

## Success Metrics

Your MVP config system is successful! ✅

- [x] 3 distinct hero types with base stats
- [x] 3 distinct weapon types with combat stats
- [x] Heroes load weapon stats from JSON
- [x] Bullets use weapon damage/speed/color
- [x] Newtonsoft.Json parsing working
- [x] ConfigManager singleton pattern
- [x] Easy to balance via JSON edits
- [x] Extensible for more heroes/weapons

**Status:** MVP Complete! 🎉

