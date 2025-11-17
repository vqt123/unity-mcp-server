# Manual Animation Testing Guide

Step-by-step guide to manually test animations in Unity Editor.

## Prerequisites

- Unity Editor open
- Character FBX files available in `Assets/Characters/FBX/`
- Animation library at `Assets/Characters/AnimationLibrary_Unity_Standard.fbx`

## Step 1: Create Test Scene

1. **File → New Scene** (or **Ctrl+N** / **Cmd+N**)
2. **File → Save Scene As...**
   - Name: `AnimationTestScene`
   - Save in: `Assets/` folder
   - **What you should see**: Scene saved in Project window

## Step 2: Load Character FBX

1. In **Project** window, navigate to `Assets/Characters/FBX/`
2. Find a character FBX (e.g., `Wizard.fbx`, `Suit_Male.fbx`)
3. **Drag the FBX** from Project window into the **Hierarchy** window
   - **What you should see**: 
     - Character appears in Hierarchy
     - Character appears in Scene view (may be at origin 0,0,0)
     - Character model visible in Scene view

## Step 3: Verify Animator Component

1. Select the character in **Hierarchy**
2. Look at **Inspector** window
3. Check for **Animator** component:
   - **What you should see**:
     - ✅ Animator component present
     - Avatar field (should show avatar name)
     - Controller field (may be empty - that's OK)
   - **If missing**: 
     - Click **Add Component** → Search "Animator" → Add
     - **What you should see**: Animator component added

## Step 4: Assign Animation Library Avatar

1. In **Project** window, navigate to `Assets/Characters/AnimationLibrary_Unity_Standard.fbx`
2. Click the **arrow** next to the FBX to expand it
3. Look for an **Avatar** asset (icon looks like a person)
4. **Drag the Avatar** onto the **Avatar** field in the Animator component
   - **What you should see**:
     - Avatar field shows avatar name (e.g., "AnimationLibrary_Unity_StandardAvatar")
     - No errors in Console
     - Avatar is marked as "Humanoid" (check the avatar's inspector)

## Step 5: Find Animation Clips

1. Still in `Assets/Characters/AnimationLibrary_Unity_Standard.fbx` in Project window
2. **Expand** the FBX (click arrow)
3. **What you should see**:
   - Multiple **AnimationClip** assets (icon looks like a film strip)
   - Names like "Rig|Idle_Loop", "Rig|Walk_Loop", etc.
   - Count should be ~90+ clips

## Step 6: Test Animation with AnimationMode (Manual Method)

### Option A: Using Animation Window (if it works)

1. **Window → Animation → Animation** (or **Ctrl+6** / **Cmd+6**)
2. Select character in Hierarchy
3. In Animation window, click **Create** button
4. **Drag an AnimationClip** from Project window into Animation window
   - **What you should see**:
     - Timeline appears in Animation window
     - Play button should be enabled
     - Character should animate when you click Play

**If Play button is disabled**: Animation Window requires Legacy clips, which won't work with modern Animator clips.

### Option B: Using AnimationMode API (What the tool uses)

This is what our tool does - it's not easily done manually, which is why we have the tool.

## Step 7: Verify Animation is Visible

1. Select character in **Hierarchy**
2. Look at **Scene** view (not Game view)
3. Click **Play** in Animation window (if using Option A)
   - **What you should see**:
     - Character's limbs/bones moving
     - Animation playing in real-time
     - Character returns to original pose when stopped

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
   - **Animation Type** should be "Humanoid"
   - **Avatar Definition** should be "Create From This Model"
2. If not Humanoid:
   - Change to "Humanoid"
   - Click **Apply**
   - Wait for import to complete
   - Avatar should appear in FBX sub-assets

### Avatar not Humanoid

1. Select the avatar asset (under AnimationLibrary_Unity_Standard.fbx)
2. Inspector should show:
   - ✅ **Is Human** = true
   - ✅ **Is Valid** = true
3. If not Humanoid:
   - Check AnimationLibrary FBX import settings
   - **Rig** tab → **Animation Type** = "Humanoid"
   - Click **Apply**

### Animation Window Play button disabled

**This is expected** - Animation Window (legacy) doesn't work with modern Animator clips.

**Solution**: Use the tool's Play button instead, which uses AnimationMode API.

### No animation clips visible

1. Navigate to `Assets/Characters/AnimationLibrary_Unity_Standard.fbx`
2. **Expand** it (click arrow)
3. Look for **AnimationClip** assets
4. **If none visible**:
   - Select the FBX
   - Inspector → **Animation** tab
   - Check **Import Animation** is enabled
   - Click **Apply**

## What Should Work

✅ **Expected working setup**:
- Character visible in Scene view
- Animator component with Humanoid avatar assigned
- Animation clips visible in AnimationLibrary FBX
- Tool's Play button plays animation (uses AnimationMode)
- Animation visible in Scene view when playing

❌ **What won't work**:
- Animation Window Play button (requires Legacy clips)
- Animator.Play() in edit mode (doesn't work outside Play mode)

## Next Steps

If manual testing works but the tool doesn't:
1. Compare what you did manually vs what the tool does
2. Check Console logs for specific errors
3. Verify each step in the tool matches the manual process

If manual testing doesn't work:
1. Check FBX import settings
2. Verify avatar is Humanoid
3. Ensure animation clips exist in the library
4. Check Unity version compatibility

