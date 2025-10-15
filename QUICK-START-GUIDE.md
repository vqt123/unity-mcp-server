# Quick Start Guide - Hero Selection Game

## ✅ Setup in 5 Minutes

### Step 1: Create Empty Scene
1. Open Unity
2. Create new scene or open `GameScene`
3. Delete everything except Main Camera and Directional Light

### Step 2: Add GameBootstrapper
1. Create empty GameObject, name it "GameBootstrap"
2. Add `GameBootstrapper` component
3. **Leave all fields empty for now** (we'll create UI next)

### Step 3: Create Hero Selection UI

1. **Create Canvas**:
   - Right-click Hierarchy → UI → Canvas
   - Name it "HeroSelectionCanvas"
   
2. **Create Selection Panel**:
   - Right-click Canvas → UI → Panel
   - Name it "HeroSelectionPanel"
   - Make it full screen or center it
   
3. **Create 3 Buttons**:
   - Right-click HeroSelectionPanel → UI → Button (TextMeshPro if available)
   - Name them: "Choice1Button", "Choice2Button", "Choice3Button"
   - Arrange them horizontally or vertically
   - Each button should have a Text child - name them "Choice1Text", "Choice2Text", "Choice3Text"

### Step 4: Wire Up Hero Selection Manager

1. Find the **HeroSelectionManager** GameObject (auto-created by GameBootstrapper)
2. In Inspector, assign:
   - Selection Panel → `HeroSelectionPanel`
   - Choice 1 Button → `Choice1Button`
   - Choice 2 Button → `Choice2Button`
   - Choice 3 Button → `Choice3Button`
   - Choice 1 Text → `Choice1Text` (the Text child of button)
   - Choice 2 Text → `Choice2Text`
   - Choice 3 Text → `Choice3Text`

### Step 5: Press Play! ▶️

**What Should Happen:**
1. Game pauses (Time.timeScale = 0)
2. Selection panel shows with 3 hero choices
3. Each button shows:
   - Hero name
   - Health, Damage, Speed stats
4. Click a hero to select
5. Panel disappears
6. Game resumes
7. Hero spawns at center
8. Wave 1 starts after 2 seconds
9. Enemies spawn in circle
10. Hero auto-shoots nearest enemy

---

## Optional: Add Prefabs for Visuals

### Create Simple Prefabs:

**Hero Prefab:**
1. Create Cube → Scale (0.5, 1, 0.5) → Color Blue
2. Add to `Assets/Prefabs/Hero.prefab`
3. Assign to EntityVisualizer → Hero Prefab

**Enemy Prefab:**
1. Create Sphere → Scale (0.5, 0.5, 0.5) → Color Red
2. Add to `Assets/Prefabs/Enemy.prefab`
3. Assign to EntityVisualizer → Enemy Prefab

**Projectile Prefab:**
1. Create Sphere → Scale (0.2, 0.2, 0.2) → Color Yellow
2. Add to `Assets/Prefabs/Projectile.prefab`
3. Assign to EntityVisualizer → Projectile Prefab

---

## Troubleshooting

### "Nothing happens when I press Play"
- Check Console for `[Bootstrap] ✅ Game setup complete!`
- Make sure HeroSelectionManager has UI references assigned
- Check that HeroSelectionPanel is Active in Hierarchy

### "I see the UI but clicking does nothing"
- Check that buttons have OnClick events wired up
- Check Console for `[HeroSelection] Player chose: ...`
- Make sure GameSimulation exists in scene

### "Hero selection works but no enemies spawn"
- Check Console for `[Wave] Hero selected, starting first wave...`
- Check that WaveManager exists (auto-created by GameBootstrapper)
- Wait 2 seconds after hero selection

### "I see nothing in the game view"
- Prefabs are optional - the simulation still runs!
- Assign Hero/Enemy/Projectile prefabs to EntityVisualizer
- Check Main Camera position (should see origin)

---

## Architecture

```
GameBootstrapper (auto-creates everything)
  ├── GameSimulation (runs 30 tps)
  ├── EntityVisualizer (creates GameObjects)
  ├── HeroSelectionManager (shows choices) ← YOU WIRE THIS
  ├── GameInitializer (disabled until hero selected)
  ├── WaveManager (disabled until hero selected)
  ├── DamageNumberSpawner
  ├── CombatEffectsManager
  ├── CameraController
  └── SimulationDebugger
```

---

## Expected Flow

1. **Start** → HeroSelectionManager shows UI (time paused)
2. **Player clicks hero** → Hero spawns at center
3. **2 seconds later** → WaveManager spawns Wave 1 (5 enemies in circle)
4. **Battle** → Hero auto-shoots, enemies move towards hero
5. **Wave complete** → Next wave starts (more enemies)
6. **Repeat** → Waves get progressively harder

---

## Hero Types Available

- `DefaultHero` - Balanced stats
- (Add more in `Assets/Shared/Data/HeroData.cs`)

---

## Quick Test (No UI)

If you want to skip hero selection:
1. Delete HeroSelectionManager GameObject
2. GameInitializer will auto-spawn `DefaultHero`
3. WaveManager will auto-start after 2 seconds

---

**That's it!** You now have a fully functional survivor-style auto-shooter with hero selection!

Next steps:
- Add more hero types
- Add upgrade UI between waves
- Add visual effects
- Add sounds
- Add networking (Phase 5)

