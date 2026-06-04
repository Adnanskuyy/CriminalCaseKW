using UnityEngine;
using UnityEngine.Video;
using CriminalCase2.Data;
using CriminalCase2.Domain;
using CriminalCase2.UI;
using CriminalCase2.Utils;
using System;
using System.Collections.Generic;

namespace CriminalCase2.Managers
{
    public class GameManager : MonoBehaviour, IGameStateProvider, IVerdictRecorder, IVideoService
    {
        public static GameManager Instance { get; private set; }

        [Header("Level Data")]
        [SerializeField] private List<LevelConfig> _levels;
        [SerializeField] private int _currentLevelIndex = 0;

        [Header("Video")]
        [SerializeField] private VideoClip _globalIntroVideo;
        [SerializeField] private string _introVideoFileName = "Videos/Intro.webm";

        [Header("Transition")]
        [SerializeField] private FadeTransition _fadeTransition;

        private GameState _currentState;
        private List<VerdictRecord> _verdictRecords = new List<VerdictRecord>();
        private bool _isTransitioning;

        public GameState CurrentState => _currentState;
        public LevelConfig? CurrentLevel => _currentLevelIndex < _levels.Count ? _levels[_currentLevelIndex] : null;
        public int CurrentLevelIndex => _currentLevelIndex;
        public IReadOnlyList<VerdictRecord> VerdictRecords => _verdictRecords.AsReadOnly();
        IReadOnlyList<VerdictRecord> IVerdictRecorder.Records => _verdictRecords.AsReadOnly();
        public int TotalLevels => _levels.Count;
        public VideoClip? GlobalIntroVideo => _globalIntroVideo;
        public string? IntroVideoFileName => _introVideoFileName;
        public bool IsTransitioning => _isTransitioning;

        public event Action<GameState>? StateChanged;
        public event Action<VerdictRecord>? VerdictRecorded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            SetState(GameState.IntroVideo);
        }

        public void SetState(GameState newState)
        {
            _currentState = newState;
            GameLogger.Info($"[GameManager] State changed to: {newState}");
            StateChanged?.Invoke(newState);
        }

        public void Record(SuspectData suspect, SuspectRole playerChoice)
        {
            _verdictRecords.RemoveAll(r => r.Suspect == suspect);
            var record = new VerdictRecord(suspect, playerChoice);
            _verdictRecords.Add(record);
            VerdictRecorded?.Invoke(record);
        }

        public void RecordVerdict(SuspectData suspect, SuspectRole playerChoice)
        {
            Record(suspect, playerChoice);
        }

        /// <summary>
        /// Advance to next level with fade transition
        /// </summary>
        public void AdvanceToNextLevel(Action onComplete = null)
        {
            if (_isTransitioning)
            {
                GameLogger.Warn("[GameManager] Already transitioning levels!");
                return;
            }

            _currentLevelIndex++;

            if (_currentLevelIndex >= _levels.Count)
            {
                GameLogger.Info("[GameManager] All levels completed!");
                _currentLevelIndex = _levels.Count - 1;
                onComplete?.Invoke();
                return;
            }

            // Perform transition
            PerformLevelTransition(onComplete);
        }

        /// <summary>
        /// Restart current level
        /// </summary>
        public void RestartCurrentLevel(Action onComplete = null)
        {
            if (_isTransitioning)
            {
                GameLogger.Warn("[GameManager] Already transitioning!");
                return;
            }

            PerformLevelTransition(onComplete);
        }

        /// <summary>
        /// Load a specific level by index
        /// </summary>
        public void LoadLevel(int levelIndex, Action onComplete = null)
        {
            if (_isTransitioning)
            {
                GameLogger.Warn("[GameManager] Already transitioning!");
                return;
            }

            if (levelIndex < 0 || levelIndex >= _levels.Count)
            {
                GameLogger.Error($"[GameManager] Invalid level index: {levelIndex}");
                return;
            }

            _currentLevelIndex = levelIndex;
            PerformLevelTransition(onComplete);
        }

        private void PerformLevelTransition(Action onComplete)
        {
            _isTransitioning = true;
            _verdictRecords.Clear();

            if (_fadeTransition != null)
            {
                // Fade out, switch level, fade in
                _fadeTransition.FadeInOut(
                    onMiddle: () =>
                    {
                        // Hide all UI panels when screen is fully black
                        UIManager.Instance?.HideAllPanels();

                        // Switch level here (while screen is black)
                        if (LevelManager.Instance != null && CurrentLevel != null)
                        {
                            LevelManager.Instance.LoadLevel(CurrentLevel);
                        }
                        // Skip video on level transition, go directly to investigation
                        SetState(GameState.Investigation);
                    },
                    onComplete: () =>
                    {
                        _isTransitioning = false;
                        onComplete?.Invoke();
                        GameLogger.Info($"[GameManager] Transition to Level {_currentLevelIndex + 1} complete!");
                    }
                );
            }
            else
            {
                // No fade transition available, just switch immediately
                // Hide all panels before switching
                UIManager.Instance?.HideAllPanels();

                if (LevelManager.Instance != null && CurrentLevel != null)
                {
                    LevelManager.Instance.LoadLevel(CurrentLevel);
                }
                // Skip video on level transition, go directly to investigation
                SetState(GameState.Investigation);
                _isTransitioning = false;
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// Check if there is a next level
        /// </summary>
        public bool HasNextLevel()
        {
            return _currentLevelIndex < _levels.Count - 1;
        }

        /// <summary>
        /// Called when player completes current level and wants to proceed
        /// </summary>
        public void OnLevelComplete()
        {
            if (HasNextLevel())
            {
                AdvanceToNextLevel();
            }
            else
            {
                GameLogger.Info("[GameManager] Game Complete! All levels finished.");
                // TODO: Show game complete screen
            }
        }

        private void OnValidate()
        {
            if (_levels == null || _levels.Count == 0)
            {
                GameLogger.Warn("[GameManager] No levels assigned in inspector.");
            }
        }
    }
}
