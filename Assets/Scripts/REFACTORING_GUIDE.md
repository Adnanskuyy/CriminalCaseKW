# Refactoring Guide: Service Locator Architecture

## Overview

This codebase has been refactored from a singleton-heavy architecture to a **Service Locator** pattern with proper separation of concerns.

## Key Changes

### 1. Service Locator Pattern (`Utils/ServiceLocator.cs`)
- **Before**: Direct singleton access (`GameManager.Instance`, `ClueManager.Instance`)
- **After**: Service registration and lookup via `ServiceLocator`

```csharp
// Old way
ClueManager.Instance.OnClueFound(clue);

// New way (recommended)
var clueService = ServiceLocator.Get<IClueService>();
clueService.FindClue(clue);
```

### 2. Logging Utility (`Utils/LoggingUtility.cs`)
- **Before**: Direct `Debug.Log()` calls everywhere
- **After**: Centralized logging with levels

```csharp
// Old way
Debug.Log("[VideoPlayerUI] Video started");

// New way
LoggingUtility.LogVideo("Video started");
// or
LoggingUtility.Info("Category", "Message");
```

### 3. Game State Machine (`Utils/GameStateMachine.cs`)
- **Before**: Polling in `Update()` method
- **After**: Event-driven state changes

```csharp
// Old way (GameStateController)
private void Update()
{
    if (currentState != _lastHandledState)
        HandleState(currentState);
}

// New way
_stateService.OnEnterClueSearch += HandleClueSearch;
```

### 4. Video Player UI (`UI/VideoPlayerUI.cs`)
- **Before**: UGUI (GameObject-based)
- **After**: UI Toolkit with IVideoPlayerService

**Key differences:**
- Uses `IVideoPlayerService` for playback logic
- Renders to `VisualElement` background texture
- Supports both VideoClip (Editor) and URL (WebGL)

### 5. Clue System
- **ClueService**: Pure C# implementation of `IClueService`
- **ClueManager**: Legacy facade for backward compatibility
- **ClueSearchUI**: Event-driven UI updates (removed `Update()` polling)

### 6. Removed Dead Code
- ✅ `ClueSearchUI.Update()` polling loop
- ✅ `SuspectClickHandler.Update()` hover animation
- ✅ Excessive `Debug.Log` statements
- ✅ State polling in `GameStateController.Update()`

## Service Interfaces

### IClueService (`Services/Interfaces/IClueService.cs`)
```csharp
public interface IClueService
{
    event Action<ClueData> OnClueFound;
    event Action OnAllCluesFound;
    IReadOnlyList<ClueData> FoundClues { get; }
    int FoundCount { get; }
    int TotalCount { get; }
    bool AllCluesFound { get; }
    
    void Initialize(ClueData[] clues);
    void FindClue(ClueData clue);
    bool IsClueFound(ClueData clue);
}
```

### IVideoPlayerService (`Services/Interfaces/IVideoPlayerService.cs`)
```csharp
public interface IVideoPlayerService
{
    event Action OnVideoStarted;
    event Action OnVideoFinished;
    event Action OnVideoSkipped;
    
    void LoadVideo(VideoClip clip);
    void LoadVideo(string url);
    void Play();
    void Stop();
    void Skip();
    
    Texture VideoTexture { get; }
    bool IsPlaying { get; }
}
```

### IGameStateService (`Services/Interfaces/IGameStateService.cs`)
```csharp
public interface IGameStateService
{
    event Action<GameState> OnStateChanged;
    event Action OnEnterClueSearch;
    event Action OnEnterDeduction;
    // ... etc
    
    void TransitionTo(GameState newState);
    bool IsInState(GameState state);
}
```

## Setup Instructions

### 1. Add Bootstrap Component
Add the `Bootstrap` component to a GameObject in your initial scene:
```csharp
GameObject bootstrap = new GameObject("Bootstrap");
bootstrap.AddComponent<Bootstrap>();
```

### 2. Configure UI Toolkit Video Panel
Create a UI Toolkit UXML structure:
```xml
<UXML>
    <VisualElement name="play-screen">
        <Label name="title-label" />
        <Label name="subtitle-label" />
        <Button name="play-button" />
    </VisualElement>
    <VisualElement name="video-screen" style="display: none;">
        <VisualElement name="video-container" />
        <Button name="skip-button" />
    </VisualElement>
</UXML>
```

### 3. Update UIManager
Ensure `UIManager` references:
- `VideoPlayerPanel` with `VideoPlayerUI` component (now uses UI Toolkit)
- All other panels as before

## WebGL Compatibility

The video player maintains WebGL compatibility through:
1. **Platform-specific compilation**:
   ```csharp
   #if UNITY_WEBGL && !UNITY_EDITOR
       LoadFromURL();
   #else
       LoadFromClip();
   #endif
   ```

2. **URL-based loading** for WebGL builds
3. **VideoClip loading** for Editor/Standalone builds
4. **APIOnly render mode** works in all platforms

## Migration Guide

### For Existing Code

**Accessing ClueService:**
```csharp
// Option 1: Direct ServiceLocator (recommended for new code)
var clueService = ServiceLocator.Get<IClueService>();

// Option 2: Legacy ClueManager (backward compatible)
ClueManager.Instance.OnClueFound(clue);
```

**Responding to State Changes:**
```csharp
// Option 1: Subscribe to events (recommended)
var stateService = ServiceLocator.Get<IGameStateService>();
stateService.OnEnterClueSearch += () => {
    // Handle clue search state
};

// Option 2: Check current state
if (stateService.IsInState(GameState.ClueSearch))
{
    // Currently in clue search
}
```

**Logging:**
```csharp
// Replace all Debug.Log calls with:
LoggingUtility.Info("Category", "Message");
LoggingUtility.LogVideo("Video event", LogLevel.Debug);
LoggingUtility.LogClue("Clue found: " + clueName);
```

## Benefits of New Architecture

1. **Testability**: Services can be mocked for unit testing
2. **Decoupling**: Components depend on interfaces, not concrete classes
3. **Flexibility**: Easy to swap implementations
4. **Performance**: Removed Update() polling loops
5. **Maintainability**: Clear separation of concerns
6. **Debugging**: Centralized logging with configurable levels

## Performance Improvements

- **Removed**: `ClueSearchUI.Update()` polling (ran every frame)
- **Removed**: `SuspectClickHandler.Update()` hover animation (now event-driven)
- **Removed**: `GameStateController.Update()` state polling (now event-driven)
- **Reduced**: Debug.Log calls (now filtered by log level)

## Next Steps

1. Test in Editor to verify functionality
2. Build for WebGL to test video playback
3. Adjust log levels for production builds:
   ```csharp
   LoggingUtility.CurrentLogLevel = LogLevel.Warning; // Production
   ```
4. Consider migrating remaining managers to services

## Troubleshooting

### Service Not Found
```csharp
// Use WhenAvailable for delayed registration
ServiceLocator.WhenAvailable<IClueService>(service => {
    // Use service
});
```

### Video Not Playing
- Check Bootstrap component is in scene
- Verify VideoPlayerService is registered
- Check log output for errors

### UI Not Updating
- Ensure UIDocument references are set
- Check USS classes match new structure
- Verify event subscriptions in OnEnable
