using UnityEngine;
using CriminalCase2.Services;
using CriminalCase2.Services.Interfaces;
using CriminalCase2.Utils;

namespace CriminalCase2.Utils
{
    /// <summary>
    /// Bootstrap component that initializes all core services at game start.
    /// Attach this to a GameObject in your initial scene.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        [Header("Logging")]
        [SerializeField] private LogLevel _initialLogLevel = LogLevel.Debug;
        [SerializeField] private bool _enableTimestamp = true;

        [Header("Service Configuration")]
        [SerializeField] private bool _registerGameStateService = true;
        [SerializeField] private bool _registerClueService = true;
        [SerializeField] private bool _registerVideoPlayerService = true;

        private void Awake()
        {
            InitializeLogging();
            InitializeServices();

            LoggingUtility.Info("Bootstrap", "All services initialized successfully");
        }

        private void InitializeLogging()
        {
            LoggingUtility.CurrentLogLevel = _initialLogLevel;
            LoggingUtility.EnableTimestamp = _enableTimestamp;

            LoggingUtility.Info("Bootstrap", $"Logging initialized with level: {_initialLogLevel}");
        }

        private void InitializeServices()
        {
            // Register GameStateService
            if (_registerGameStateService && !ServiceLocator.IsRegistered<IGameStateService>())
            {
                var gameStateService = new GameStateService();
                ServiceLocator.Register<IGameStateService>(gameStateService);
                LoggingUtility.LogDebug("Bootstrap", "GameStateService registered");
            }

            // Register ClueService
            if (_registerClueService && !ServiceLocator.IsRegistered<IClueService>())
            {
                var clueService = new ClueService();
                ServiceLocator.Register<IClueService>(clueService);
                LoggingUtility.LogDebug("Bootstrap", "ClueService registered");
            }

            // Register VideoPlayerService (requires MonoBehaviour, so we create a GameObject)
            if (_registerVideoPlayerService && !ServiceLocator.IsRegistered<IVideoPlayerService>())
            {
                var videoService = FindFirstObjectByType<VideoPlayerService>();
                if (videoService == null)
                {
                    var go = new GameObject("VideoPlayerService");
                    videoService = go.AddComponent<VideoPlayerService>();
                    DontDestroyOnLoad(go);
                }
                ServiceLocator.Register<IVideoPlayerService>(videoService);
                LoggingUtility.LogDebug("Bootstrap", "VideoPlayerService registered");
            }
        }
    }
}
