# Manual Animation Testing Guide

**Last Updated**: January 2025  
**Current System**: Animations come directly from character FBX files

---

## Prerequisites

- Unity Editor open
- Character FBX files available in `Assets/Characters/FBX/`
- Animations are embedded in the FBX files (e.g., `BaseCharacter.fbx`)

---

## Step 1: Create Test Scene

1. **File → New Scene** (or **Ctrl+N** / **Cmd+N**)
2. **File → Save Scene As...**
   - Name: `AnimationTestScene`
   - Save in: `Assets/` folder
   - **What you should see**: Scene saved in Project window

---

## Step 2: Load Character FBX

1. In **Project** window, navigate to `Assets/Characters/FBX/`
2. Find a character FBX (e.g., `BaseCharacter.fbx`, `BlueSoldier_Female.fbx`)
3. **Drag the FBX** from Project window into the **Hierarchy** window
   - **What you should see**: 
     - Character appears in Hierarchy
     - Character appears in Scene view (may be at origin 0,0,0)
     - Character model visible in Scene view

---

## Step 3: Verify Animator Component

1. Select the character in **Hierarchy**
2. Look at **Inspector** window
3. Check for **Animator** component:
   - **What you should see**:
     - ✅ Animator component present (may be added automatically)
     - Avatar field (may be empty or show avatar name)
     - Controller field (may be empty - that's OK)
   - **If missing**: 
     - Click **Add Component** → Search "Animator" → Add
     - **What you should see**: Animator component added

---

## Step 4: Find Animation Clips in FBX

1. In **Project** window, navigate to your character FBX (e.g., `BaseCharacter.fbx`)
2. **Expand** the FBX (click arrow next to it)
3. **What you should see**:
   - Multiple **AnimationClip** assets (icon looks like a film strip)
   - Names like `CharacterArmature|Idle`, `CharacterArmature|Walk`, `CharacterArmature|Shoot_OneHanded`, etc.
   - These are the animations embedded in the FBX

---

## Step 5: Test Animation in Play Mode

### Method 1: Using Animator Controller (Recommended)

1. **Create Animator Controller**:
   - Right-click in Project → `Create → Animator Controller`
   - Name it `TestController.controller`

2. **Open Animator Window**:
   - **Window → Animation → Animator** (or **Ctrl+6** / **Cmd+6**)

3. **Add Animation States**:
   - In Animator window, right-click → `Create State → Empty`
   - Name it "Idle"
   - Select the state, then in Inspector drag an animation clip (e.g., `CharacterArmature|Idle`) to the **Motion** field

4. **Assign Controller to Character**:
   - Select character in Hierarchy
   - Drag `TestController.controller` to **Controller** field in Animator component

5. **Enter Play Mode**:
   - Click **Play** button
   - Character should animate!

### Method 2: Using Animation Window (Limited)

1. **Window → Animation → Animation** (or **Ctrl+6** / **Cmd+6**)
2. Select character in Hierarchy
3. In Animation window, click **Create** button
4. **Drag an AnimationClip** from the FBX into Animation window
   - **What you should see**:
     - Timeline appears in Animation window
     - Play button should be enabled
     - Character should animate when you click Play

**Note**: Animation Window may not work with all animation types. Use Method 1 for more reliable testing.

---

## Step 6: Verify Animation is Visible

1. Select character in **Hierarchy**
2. Look at **Scene** view (not Game view)
3. Enter **Play Mode** (or use Animation window Play button)
   - **What you should see**:
     - Character's limbs/bones moving
     - Animation playing in real-time
     - Character returns to original pose when stopped

---

## Troubleshooting

### Character not visible in Scene view

1. Check **Scene** view camera position:
   - Select **Main Camera** in Hierarchy
   - Look at Inspector - **Position** should be near (0, 0, -10)
   - **If character is at origin**: Camera should see it
2. **Frame** the character:
   - Select character in Hierarchy
   - Press **F** key (or **right-click Scene view → Frame Selected**)
   - **What you should see**: Camera zooms to show character

### Animator has no Avatar

1. Check FBX import settings:
   - Select character FBX in Project
   - Inspector → **Rig** tab
   - **Animation Type** can be:
     - **Generic** (uses model's bone structure directly)
     - **Humanoid** (requires Humanoid avatar)
     - **Legacy** (old animation system)
   - **Avatar Definition** should be "Create From This Model" (if Humanoid)
2. If using Generic:
   - Avatar is not required
   - Animations use the model's native bone structure

### No animation clips visible

1. Navigate to your character FBX in Project window
2. **Expand** it (click arrow)
3. Look for **AnimationClip** assets
4. **If none visible**:
   - Select the FBX
   - Inspector → **Animation** tab
   - Check **Import Animation** is enabled
   - Click **Apply**
   - Wait for reimport to complete

### Animation Window Play button disabled

**This is expected** - Animation Window (legacy) doesn't work with all modern Animator clips.

**Solution**: Use Play Mode with an Animator Controller (Method 1 above).

### Animations not looping

1. Select the animation clip in Project window
2. Inspector → Check **Loop Time** checkbox
3. Click **Apply**

Or use the **"Configure Animation Loop Settings"** button in `GlobalGameSettings` Inspector.

---

## What Should Work

✅ **Expected working setup**:
- Character visible in Scene view
- Animator component present
- Animation clips visible in FBX (expand to see)
- Animator Controller with animation states
- Animation plays in Play Mode

❌ **What won't work**:
- Animation Window Play button (may not work with all animation types)
- Animator.Play() in edit mode (doesn't work outside Play mode)

---

## Testing with GlobalGameSettings

To test the actual game animation system:

1. **Configure GlobalGameSettings**:
   - Open `Assets/Resources/GlobalGameSettings.asset`
   - Assign `defaultHeroModel` (e.g., `BaseCharacter.fbx`)
   - Assign `heroIdleAnimation`, `heroWalkAnimation`, `heroFireAnimation`
   - Click **"Create Hero Base Animator Controller"** if needed

2. **Test in Game Scene**:
   - Open `Assets/Scenes/ArenaGame.unity`
   - Enter Play Mode
   - Heroes should spawn with animations from GlobalGameSettings

3. **Check Console Logs**:
   - Look for `[animtest]` logs
   - These show animation setup progress

---

## Next Steps

If manual testing works but game animations don't:
1. Check `GlobalGameSettings` configuration
2. Verify `HeroBaseController.controller` exists
3. Check console for `[animtest]` error logs
4. Compare manual setup vs game setup

If manual testing doesn't work:
1. Check FBX import settings
2. Verify animation clips exist in FBX
3. Ensure Animator Controller is properly configured
4. Check Unity version compatibility

---

## Related Documentation

- **Using-Hero-Prefabs-In-Game.md**: How to configure heroes in-game
- **GlobalGameSettings.cs**: Current animation system
- **EntityVisualizer.cs**: Runtime animation setup code
