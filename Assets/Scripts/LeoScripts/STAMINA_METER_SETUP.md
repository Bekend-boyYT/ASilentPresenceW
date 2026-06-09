# Stamina Meter UI - Setup Guide

## Overview
The `StaminaMeterUI` script displays a real-time stamina meter showing:
- **Stamina Slider**: A built-in UI Slider that fills/depletes based on current stamina (colors: green → yellow → red → dark red)
- **Stamina Text**: Shows percentage (0-100%)
- **Breathing State**: Shows current breathing state (CALM → MEDIUM → HEAVY → EXHAUSTED)
- **Auto-fade**: Fades to low opacity when stamina is full, becomes more visible as stamina depletes

## Setup Instructions

### Step 1: Create the UI Canvas
1. In your scene, right-click in the Hierarchy → **UI → Panel** (this creates a Canvas + Panel)
2. Rename the Panel to `StaminaMeterUI`
3. Position it in a corner (e.g., bottom-left or top-right) using RectTransform

### Step 2: Create the Stamina Slider
1. Select the `StaminaMeterUI` panel
2. Right-click → **UI → Slider**
3. Rename it to `StaminaSlider`
4. This creates a Slider with Background, Fill, and Handle automatically
5. Position and resize as desired (e.g., 200px wide, 20px tall in a corner)
6. Select the Slider and in the Inspector:
   - Set **Max Value** to **1.0** (it represents 0-1 ratio)
   - Set **Min Value** to **0.0**
   - **Uncheck** "Interactable" (it's display-only, player can't interact)
   - The Fill will animate automatically as the value changes

### Optional: Customize Slider Colors
1. Expand the Slider in the Hierarchy to see its child components:
   - **Background**: The background image
   - **Fill Area → Fill**: The bar that fills (this will change color based on stamina)
   - **Handle Slide Area → Handle**: The draggable handle (you can hide this or customize)
2. Select the **Fill** image and adjust its color in the Inspector if you want a default color

### Step 3: Create Status Text (Old Step 4, renumbered)
1. Select the `StaminaMeterUI` panel
2. Right-click → **UI → TextMeshPro - Text**
3. Rename it to `StatusText`
4. Set the text to something like "100% CALM"
5. Position it below or next to the slider
6. Adjust font size and colors as desired

### Step 4: Add the Script (Old Step 5, renumbered)
1. Select `StaminaMeterUI` (the parent panel)
2. Add the **StaminaMeterUI** script component (drag it in, or use **Add Component** → search for it)

### Step 5: Configure the Script (Old Step 6, renumbered)
In the Inspector for the `StaminaMeterUI` component:

| Field | What to Connect |
|-------|-----------------|
| **Player Controller** | Drag your player object (with FirstPersonController script) here. *If left empty, it will auto-find.* |
| **Stamina Slider** | Drag the `Slider` component here (or its Transform) |
| **Stamina Text** | Drag the TextMeshPro text showing "%" here |
| **Breathing State Text** | (Optional) Drag a separate text element here if you want CALM/MEDIUM/HEAVY/EXHAUSTED displayed separately |
| **Colors** | Adjust High/Medium/Low/Exhausted colors to your preference |
| **Fade Speed** | How quickly the UI fades in/out (default 2.0 is good) |
| **Min/Max Alpha** | How invisible/visible the UI gets (0.3 to 1.0 is recommended) |

### Step 6: Test (Old Step 7, renumbered)
1. Play the game
2. Sprint around and watch the bar deplete and the breathing state change
3. Stop sprinting and watch it regenerate
4. The UI should fade to low opacity when at full stamina

## Understanding the Breathing States

| State | Stamina Range | What's Happening |
|-------|---------------|------------------|
| **CALM** | 80-100% | Player is rested, breathing normally |
| **MEDIUM** | 50-80% | Light exertion, moderate breathing |
| **HEAVY** | 20-50% | Heavy exertion from sprinting |
| **EXHAUSTED** | 0-20% | Out of stamina, heavily exhausted |

## Customization Tips

### Change the bar colors:
- Adjust `HighStaminaColor`, `MediumStaminaColor`, `LowStaminaColor`, and `ExhaustedColor` in the script or Inspector

### Show breathing state separately:
- Create a second TextMeshPro text for breathing state only
- Assign it to the **Breathing State Text** field
- This text will show "CALM" / "MEDIUM" / "HEAVY" / "EXHAUSTED" independently

### Adjust fade behavior:
- Increase `FadeSpeed` to make fading faster
- Decrease `MinAlpha` to make it fade more (less visible when not in use)
- Increase `MaxAlpha` to keep it brighter

### Position presets:
- **Top-Left**: Good for debugging
- **Bottom-Right**: Classic action game style
- **Center-Bottom**: Less intrusive, good for immersion

## Troubleshooting

**"FirstPersonController not found!" warning**
- Make sure your player object has the `FirstPersonController` script attached
- Either drag it into the `Player Controller` field, or make sure it's in the scene

**Slider not changing value**
- Make sure the `Stamina Slider` field is properly assigned
- Check that the Slider's **Max Value** is set to **1.0**
- Verify **Interactable** is unchecked (so the player can't manually drag it)

**Slider colors not changing**
- The script automatically colors the **Fill** image based on stamina
- Make sure the Slider's Fill component exists (it should by default)
- Check that the **Fill** Image is a child of the Slider

**Text not updating**
- Make sure the TextMeshPro component is properly assigned
- Check that the text is using TextMeshPro, not the legacy Text component

**UI not fading**
- Make sure the parent Canvas has a **CanvasGroup** component (the script adds one automatically)
- Check the `FadeSpeed` and min/max alpha values

