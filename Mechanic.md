# Game Mechanics & Design Specification

## Overview

A Criminal Case-style investigation game where players search crime scenes for hidden clues, then use those clues to deduce which suspect is the Drug User (Pecandu), the Drug Dealer (Bandar Narkoba), and who is an Innocent Bystander (Warga Biasa). The game is built in Unity 6.3 LTS using UI Toolkit and the New Input System.

## Core Gameplay Flow

Each level follows three distinct phases:

1. **Intro Video:** Plays automatically on every level (Skippable and each level has different video).
2. **Tutorial:** Overlay explains controls. Can be reopened via Tutorial Button.
3. **Clue Search Phase:**
   - The crime scene is displayed with suspects **hidden**.
   - Clue objects are scattered and partially hidden throughout the scene.
   - Player taps/clicks on clue objects to collect them.
   - A bottom panel shows collected clue icons (silhouettes for unfound, full color when found).
   - A counter shows progress: "Petunjuk Ditemukan: X/N".
   - No time limit — player searches at their own pace.
   - When all clues are found, the "Lanjut ke Deduksi" (Proceed to Deduction) button appears.
4. **Deduction Phase — Sub-Phase A: Clue Matching:**
   - Four suspect cards are displayed.
   - Found clues appear as draggable/tappable items below the suspect cards.
   - Player selects a clue, then taps a suspect card to assign the clue to that suspect.
   - Each suspect can receive multiple clues.
   - Clues can be reassigned by tapping them on a suspect card to return them to the pool.
   - Each clue is secretly linked to one suspect; correct matching matters for scoring.
   - Player confirms matching with "Konfirmasi Cocokkan Petunjuk" button.
5. **Deduction Phase — Sub-Phase B: Role Assignment:**
   - Each suspect card now shows their matched clues as context.
   - Player assigns a role to each suspect: Pecandu (User), Bandar Narkoba (Dealer), or Warga Biasa (Normal).
   - Drug Test: Player can use up to 2 Drug Tests per level on any suspect to reveal their test result (Positive/Negative).
   - A Drug Test button appears on each suspect card; tapping it consumes one test and reveals the result.
   - "Kirim Vonis Akhir" (Submit Final Verdict) button is enabled when all suspects have a role assigned.
6. **Results:** Shows each suspect's matched clues, player role choice vs correct answer, clue matching accuracy, and feedback text.
7. **Progression:** Move to next level.

## UI Systems (Unity UI Toolkit)

### 1. Video Player UI

- **Components:** Video Render Texture, Skip Button.
- **Behavior:** Auto-play on every level with each video. Skip button ends video early.

### 2. Tutorial UI

- **Components:** Text instructions, Close Button.
- **Trigger:** Appears after video or via persistent Tutorial Button.
- **Function:** Can replay the intro video.

### 3. Clue Search UI

- **Layout:**
  - **Main Area:** Crime scene with interactive clue objects.
  - **Bottom Panel:** Clue inventory showing collected clue icons.
  - **Counter:** "Petunjuk Ditemukan: X/N" at the top.
  - **Proceed Button:** "Lanjut ke Deduksi" — appears only when all clues are found.
- **Interaction:**
  - Tapping a hidden clue object plays a "found" animation and adds it to the inventory.
  - Found clues are highlighted in the inventory (silhouette → full color icon).
  - Already-found clues cannot be tapped again.

### 4. Deduction UI (Two Sub-Views)

#### 4a. Clue Matching View

- **Layout:**
  - **Top:** Four suspect cards in a row (portrait + name, initially with no clues).
  - **Bottom:** Pool of found clues as tappable items.
- **Interaction:**
  - Tap a clue item → it becomes "selected" (highlighted).
  - Tap a suspect card → the selected clue is assigned to that suspect and appears on the card.
  - Tap a clue on a suspect card → it returns to the pool.
  - Each suspect card shows matched clues as small icons below the portrait.
- **Confirmation:** "Konfirmasi Cocokkan Petunjuk" button to proceed to role assignment.

#### 4b. Role Assignment View

- **Layout:**
  - **Top:** Four suspect cards showing their matched clues.
  - **Below Each Card:** Three role buttons (Pecandu / Bandar Narkoba / Warga Biasa) + Drug Test button.
  - **Drug Test Counter:** "Tes Narkoba Tersisa: X/2" shown globally.
- **Interaction:**
  - Player selects one role per suspect.
  - Drug Test button reveals Positive/Negative result on the suspect card. Limited to 2 uses per level.
  - "Kirim Vonis Akhir" button enabled when all 4 suspects have a role assigned.

### 5. Suspect Detail UI (Popup) — Used During Role Assignment

- **Layout:**
  - **Left:** Suspect Portrait, Drug Test Button, Drug Test Result Text.
  - **Right:** Description Text, Matched Clue Evidence.
  - **Bottom:** Role Selection Buttons (Pecandu/Bandar Narkoba/Warga Biasa).
- **Interaction:**
  - Clicking a role button saves the choice and closes the popup.
  - Drug Test button uses one remaining test and reveals the result.

### 6. Result UI

- **Content:** For each suspect: name, matched clues, player role choice, correct role, feedback text.
- **Clue Matching Score:** Shows how many clues were correctly assigned.
- **Overall Score:** Percentage of correct verdicts and clue matchings.
- **Navigation:** "Level Berikutnya" (Next Level) button.

## Game Objects & Scene Structure

### Level Prefab

- **Background:** Static GameObject (Visual only) — crime scene image.
- **Clue Objects:** Interactive GameObjects placed at positions defined by `ClueData.scenePosition`.
  - **SpriteRenderer:** Displays the clue sprite (small, semi-hidden in scene).
  - **Collider2D:** Required for tap detection.
  - **ClueClickHandler:** MonoBehaviour handling tap interaction and "found" animation.
  - **Initially Dimmed:** Slightly transparent or small; animates to full visibility when found.
- **Suspect Objects:** 4 GameObjects per level — **initially hidden** during Clue Search phase.
  - **Collider:** Required for clicking in Deduction.
  - **Visual:** 2D representation of the suspect.
  - **Polish:** On Hover → Highlight Material, Scale Up, Slight Rotation.
  - **Activated:** Only when transitioning to Deduction phase.
- **Manager:** `ClueManager` tracks found clues; `LevelManager` handles suspect lifecycle.

## Data Structures

### ClueData (ScriptableObject)

| Field                | Type    | Description                                                                               |
| :------------------- | :------ | :---------------------------------------------------------------------------------------- |
| `clueName`           | string  | Display name of the clue.                                                                 |
| `description`        | string  | Flavor text shown when clue is examined.                                                  |
| `clueSprite`         | Sprite  | Sprite displayed in the scene and inventory.                                              |
| `clueIcon`           | Sprite  | Smaller icon for inventory panel.                                                         |
| `scenePosition`      | Vector2 | Position in the scene where the clue appears.                                             |
| `linkedSuspectIndex` | int     | Index into `LevelConfig._suspects[]` (0–3). Indicates which suspect this clue belongs to. |
| `isDrugTestClue`     | bool    | If true, finding this clue grants an extra Drug Test.                                     |

### SuspectData (ScriptableObject)

| Field                 | Type    | Description                                     |
| :-------------------- | :------ | :---------------------------------------------- |
| `suspectName`         | string  | Name displayed in UI.                           |
| `description`         | string  | Main bio text.                                  |
| `evidenceText`        | string  | Clue text (shown if no matched clues).          |
| `evidenceImage`       | Texture | Default clue image (shown if no matched clues). |
| `portrait`            | Texture | Image for Detail UI.                            |
| `drugTestResult`      | enum    | Positive or Negative.                           |
| `correctRole`         | enum    | User, Dealer, Normal.                           |
| `feedbackTextCorrect` | string  | Shown in Result UI if verdict is correct.       |
| `feedbackTextWrong`   | string  | Shown in Result UI if verdict is wrong.         |

### LevelConfig (ScriptableObject)

| Field                  | Type           | Description                                            |
| :--------------------- | :------------- | :----------------------------------------------------- |
| `levelIndex`           | int            | Level number.                                          |
| `levelName`            | string         | Level display name.                                    |
| `backgroundSprite`     | Sprite         | Crime scene background.                                |
| `levelPrefab`          | GameObject     | Prefab containing scene layout + clue/suspect objects. |
| `suspects`             | SuspectData[4] | Four suspects for this level.                          |
| `clues`                | ClueData[]     | Clues hidden in this level's scene.                    |
| `maxDrugTestsPerLevel` | int            | Default: 2. Drug tests available for role assignment.  |

## Mechanics Details

### Clue Search Mechanic

- **Objective:** Find all hidden clue objects in the crime scene.
- **Interaction:** Tap/click on a clue object to collect it.
- **Feedback:** Found clue plays a discovery animation (scale up, glow, fade to inventory).
- **Progress:** Side panel tracks found vs. total clues.
- **Completion:** When all clues are found, the proceed button appears.
- **No Time Limit:** Player can search at their own pace.

### Clue Matching Mechanic

- **Objective:** Assign each found clue to the correct suspect.
- **Interaction:** Select a clue from the pool, then tap a suspect card.
- **Reassignment:** Clues can be moved between suspects by tapping them on a suspect card.
- **Scoring:** Correct clue-to-suspect assignments contribute to the final score.
- **Confirmation:** Player must confirm all clue assignments before proceeding to role assignment.

### Drug Test Mechanic

- **Action:** Player clicks "Gunakan Tes Narkoba" button on a suspect card or detail popup.
- **Result:** Reveals `drugTestResult` (Positif/Negatif) on the suspect card.
- **Constraint:** Maximum 2 uses per level (configurable via `LevelConfig.maxDrugTestsPerLevel`).
- **Timing:** Available only during the Role Assignment sub-phase of Deduction.

### Role Assignment (Verdict) Mechanic

- **Options per Suspect:**
  1. Pecandu (Drug User)
  2. Bandar Narkoba (Drug Dealer)
  3. Warga Biasa (Normal Person)
- **Validation:** Checked against `SuspectData.correctRole`.
- **Prerequisite:** All clues must be matched before role assignment is available.

### Scoring

- **Clue Matching Accuracy:** Number of clues correctly assigned to their linked suspect vs. total clues.
- **Verdict Accuracy:** Number of suspects correctly identified vs. total suspects.
- **Overall Result:** Combined score displayed in Result UI.

### Level Management

- **State Tracking:** `ClueManager` tracks found clues; `DeductionManager` tracks clue assignments and verdicts.
- **Completion Check:** Results trigger when all 4 suspects have a role assigned.
- **Loading:** Loads next Level Prefab based on level index.
- **Suspect Reveal:** Suspect GameObjects are hidden during Clue Search, activated when entering Deduction.

## Game State Machine

```
IntroVideo → Tutorial → ClueSearch → Deduction → Results → (next level)
                                        ├─ SubPhase: Matching
                                        └─ SubPhase: RoleAssignment
```

### State Definitions

| State        | Description                                               |
| :----------- | :-------------------------------------------------------- |
| `IntroVideo` | Video playback. Transition on video end or skip.          |
| `Tutorial`   | Tutorial overlay. Transition on close.                    |
| `ClueSearch` | Crime scene with hidden clues. Transition when all found. |
| `Deduction`  | Two sub-phases: Clue Matching → Role Assignment.          |
| `Results`    | Show all verdicts, clue matching, and feedback.           |

## Input Mapping (New Input System)

- **Pointer Click:** Interact with Clues, Suspects, and UI Buttons.
- **Pointer Hover:** Trigger clue highlight effects and suspect highlight effects.
- **Cancel/Escape:** Return clue to pool (during matching), close popups.
