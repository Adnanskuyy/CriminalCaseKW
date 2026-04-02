# Game Mechanics & Design Specification

## Overview

An investigation game where players analyze suspects to determine if they are Drug Users, Drug Dealers, or Normal Citizens. The game is built in Unity 6.3 LTS using UI Toolkit and the New Input System.

## Core Gameplay Flow

1. **Intro Video:** Plays automatically on level start (Skippable).
2. **Tutorial:** Overlay explains controls. Can be reopened via Tutorial Button.
3. **Investigation:**
   - Player views 4 Suspects in the scene.
   - Player clicks a Suspect -> Opens Detail UI.
   - Player reads Description & Evidence.
   - Player optionally uses "Drug Test" (Shows Result).
   - Player selects Verdict (User/Dealer/Normal).
4. **Completion:** All 4 suspects must be judged.
5. **Results:** Shows summary of choices, correct answers, and feedback.
6. **Progression:** Move to next level.

## UI Systems (Unity UI Toolkit)

### 1. Video Player UI

- **Components:** Video Render Texture, Skip Button.
- **Behavior:** Auto-play on level load. Skip button ends video early.

### 2. Tutorial UI

- **Components:** Text instructions, Close Button.
- **Trigger:** Appears after video or via persistent Tutorial Button.
- **Function:** Can replay the intro video.

### 3. Suspect Detail UI (Popup)

- **Layout:**
  - **Left:** Suspect Portrait, Drug Test Button, Drug Test Result Text.
  - **Right:** Description Text, Evidence Text.
  - **Bottom:** Evidence Image (Clickable to enlarge), Verdict Buttons (User/Dealer/Normal).
- **Interaction:**
  - Clicking Evidence Image opens a full-screen preview.
  - Clicking Verdict buttons saves the choice and closes the popup.

### 4. Result UI

- **Content:** List of all suspects, Player Choice, Correct Answer, Feedback Text.
- **Navigation:** "Next Level" button.

## Game Objects & Scene Structure

### Level Prefab

- **Background:** Static GameObject (Visual only).
- **Suspects:** 4 Interactive GameObjects per level.
  - **Collider:** Required for clicking.
  - **Visual:** 3D/2D representation of the suspect.
  - **Polish:** On Hover -> Highlight Material, Scale Up, Slight Rotation.
- **Manager:** `LevelManager` instance handles state.

### Interactables

- **Clickable:** Suspect GameObjects, Tutorial Button.
- **Non-Clickable:** Background, Decorations.

## Data Structure (ScriptableObject: SuspectData)

| Field            | Type      | Description                        |
| :--------------- | :-------- | :--------------------------------- |
| `suspectName`    | string    | Name displayed in UI.              |
| `description`    | string    | Main bio text.                     |
| `evidenceText`   | string    | Clue text (optional).              |
| `evidenceImage`  | Texture   | Clue image (optional).             |
| `portrait`       | Texture   | Image for Detail UI.               |
| `drugTestResult` | bool/enum | Positive or Negative.              |
| `correctRole`    | Enum      | User, Dealer, Normal.              |
| `feedbackText`   | string    | Shown in Result UI if wrong/right. |

## Mechanics Details

### Drug Test Mechanic

- **Action:** Player clicks "Use Drug Test" button in Detail UI.
- **Result:** Reveals `drugTestResult` (Positive/Negative).
- **Constraint:** Can be used once per suspect or unlimited (configurable).

### Verdict Mechanic

- **Options:**
  1. Pengguna Narkoba (Drug User)
  2. Pengedar Narkoba (Drug Dealer)
  3. Orang Biasa (Normal Person)
- **Validation:** Checked against `correctRole` in `SuspectData`.

### Level Management

- **State Tracking:** Tracks which suspects have been judged.
- **Completion Check:** Triggers Result UI when all 4 suspects are judged.
- **Loading:** Loads next Level Prefab based on level index.

## Input Mapping (New Input System)

- **Pointer Click:** Interact with Suspects/UI Buttons.
- **Pointer Hover:** Trigger suspect highlight effects.
- **Cancel/Escape:** Close popups (if applicable).
