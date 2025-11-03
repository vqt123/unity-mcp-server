# Coroutine Alternatives & Best Practices

## Current Coroutine Use Cases

### ✅ **Good Uses (Keep Coroutines)**
1. **Simple timed delays** (WaveManager, wave delays)
   - Coroutines are perfect for this
   - Alternative: Timer class, but coroutines are simpler

2. **Infinite loops with intervals** (EnergyManager regeneration)
   - Coroutines work well
   - Alternative: Timer class or Update() with time tracking

### 🔄 **Better Alternatives Available**

#### 1. **Hero Finding - Use Events Instead of Polling**

**Current (Coroutine Polling):**
```csharp
// XPBarUI.cs - Polling in coroutine
StartCoroutine(FindHeroDelayed());
while (!heroId.IsValid && attempts < 20) {
    if (world.HeroIds.Count > 0) { ... }
    yield return null;
}
```

**Better (Event-Driven):**
```csharp
// XPBarUI.cs - Subscribe to HeroSpawnedEvent
void OnEnable() {
    EventBus.Subscribe<HeroSpawnedEvent>(OnHeroSpawned);
}

void OnHeroSpawned(ISimulationEvent evt) {
    if (evt is HeroSpawnedEvent spawn) {
        heroId = spawn.HeroId;
        trackingHero = true;
        EventBus.Unsubscribe<HeroSpawnedEvent>(OnHeroSpawned); // One-time
    }
}
```

**Benefits:**
- No polling/waiting
- Instant response
- No race conditions
- More performant (no wasted frames)

#### 2. **Upgrade Flow - Use State Machine or Event Chain**

**Current (Coroutine Chaining):**
```csharp
OnHeroSelectedFromUpgrade() 
  → StartCoroutine(TriggerInitialLevelUp()) 
    → Wait for hero 
      → Level up 
        → Event fires 
          → Show panel
```

**Better (Event-Driven Chain):**
```csharp
// Subscribe to HeroSpawnedEvent when hero is selected
OnHeroSelectedFromUpgrade() {
    dataMgr.IncrementUpgradesAtLevel1();
    // Subscribe to next event in chain
    EventBus.Subscribe<HeroSpawnedEvent>(OnHeroSpawnedForLevelUp);
    spawner.SpawnSelectedHero(heroType);
}

void OnHeroSpawnedForLevelUp(ISimulationEvent evt) {
    EventBus.Unsubscribe<HeroSpawnedEvent>(OnHeroSpawnedForLevelUp);
    // Immediately trigger level up
    TriggerLevelUpTo1(heroId);
    // Subscribe to level-up event
    EventBus.Subscribe<HeroLevelUpEvent>(OnHeroLevelUpForSecondUpgrade);
}

void OnHeroLevelUpForSecondUpgrade(ISimulationEvent evt) {
    if (upgradesChosen == 1) {
        ShowUpgradePanelForLevel1SecondUpgrade();
    }
}
```

**Benefits:**
- Clear event flow
- No timing issues
- Easier to debug
- Self-documenting

#### 3. **Initialization Delays - Use Proper Initialization Order**

**Current:**
```csharp
Invoke(nameof(TriggerInitialLevelUpForNoHero), 0.3f);
```

**Better:**
```csharp
// In GameBootstrapper, ensure UpgradeUIManager initializes last
// OR use a ready event
void Start() {
    EventBus.Subscribe<GameInitializedEvent>(OnGameReady);
}

void OnGameReady(ISimulationEvent evt) {
    ShowUpgradePanelForLevel1FirstUpgrade();
}
```

## Recommendations

### Keep Coroutines For:
- ✅ Simple timed delays (< 5 seconds)
- ✅ Infinite loops with intervals (energy regen)
- ✅ Animation sequences

### Use Events For:
- ✅ Entity spawning/despawning
- ✅ State changes (level up, upgrade chosen)
- ✅ Multi-step workflows
- ✅ Waiting for other systems

### Use State Machines For:
- ✅ Complex UI flows (upgrade system)
- ✅ Game state management
- ✅ AI behavior

## Refactoring Priority

1. **High Priority:** Replace hero-finding polling with `HeroSpawnedEvent` subscription
   - XPBarUI
   - TestRadialProgress  
   - UpgradeUIManager (waiting for hero spawn)

2. **Medium Priority:** Refactor upgrade flow to use event chain
   - Cleaner, more maintainable
   - Eliminates race conditions

3. **Low Priority:** Keep wave delays and energy regen as coroutines
   - They're simple and work well
   - No clear benefit to changing

## Performance Note

Events are more performant than coroutines for reactive code:
- **Coroutines:** Poll every frame until condition met (wasted CPU)
- **Events:** Instant callback when condition occurs (0 wasted CPU)

