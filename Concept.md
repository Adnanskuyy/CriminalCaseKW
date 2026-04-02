Act as a Lead Unity Architect. Help me build a Detective Investigation game in Unity 6.3 LTS using UI Toolkit and WebGL. 

### Game Concept: 
A drug-testing investigation where players analyze 4 suspects (Polaroid view) to determine their status (Pengguna narkoba, Pengedar narkoba, or Orang biasa). 

### Core Architecture Requirements (SOLID):
1. **Data-Driven (S/O):** Use ScriptableObjects for `SuspectData`. It must contain:
   - Suspect Name, Portrait Sprite.
   - Physical Characteristics (Text + Optional sprite).
   - Optional Evidence(Text + Sprite)
   - Hidden Truth (Boolean: Is actually using drug?).
2. **Single Responsibility (S):** - `InvestigationManager`: Handles game state (Urine test used, Verdicts submitted).
   - `SuspectClickable`: Handles when suspect object is clicked on level
   - `DetailPanelController`: Manages the pop-up info when a photo is clicked.
3. **Dependency Inversion (D):** Use an Interface `IEvidence` for characteristics/behavior so new types of evidence can be added later without changing the UI logic.
4. **State Constraints:** The "Urine Test" is a singleton-action per session. Track its 'Used' state globally.

### UI Toolkit Specifications:
- Create a `SuspectTemplate.uxml` for the Polaroid photo to ensure scalability.
- Use `USS` for hover effects on the photos.
- Use `Runtime Data Binding` (Unity 6 feature) to link ScriptableObject data to the UI labels and sprites.

### Your Task:
1. Propose a folder structure following industry standards (Assets/Scripts/Data, Assets/UI/UXML, etc.).
2. Define the C# class structure for the `SuspectData` ScriptableObject.
3. Explain how to set up the UI Toolkit `PanelSettings` for a responsive WebGL layout.

Begin by analyzing the project structure and drafting the ScriptableObject schema.