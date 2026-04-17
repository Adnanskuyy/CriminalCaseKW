using UnityEngine;
using UnityEngine.Video;
using CriminalCase2.Data;
using CriminalCase2.Services;
using CriminalCase2.Services.Interfaces;
using CriminalCase2.UI;
using CriminalCase2.Utils;
using System.Collections.Generic;
using System;

namespace CriminalCase2.Managers
{
    public class GameManager : MonoBehaviour
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

        private IGameStateService _stateService;
        private List<VerdictRecord> _verdictRecords = new List<VerdictRecord>();
        private bool _isTransitioning;

        public GameState CurrentState => _stateService?.CurrentState ?? GameState.IntroVideo;
        public LevelConfig CurrentLevel => _currentLevelIndex < _levels.Count ? _levels[_currentLevelIndex] : null;
        public int CurrentLevelIndex => _currentLevelIndex;
        public IReadOnlyList<VerdictRecord> VerdictRecords => _verdictRecords.AsReadOnly();
        public int TotalLevels => _levels.Count;
        public VideoClip GlobalIntroVideo => _globalIntroVideo;
        public string IntroVideoFileName => _introVideoFileName;
        public bool IsTransitioning => _isTransitioning;

        public float ClueMatchingAccuracy
        {
            get
            {
                var matchingService = ServiceLocator.Get<IClueMatchingService>();
                return matchingService?.MatchingAccuracy ?? 0f;
            }
        }
        public int CorrectClueMatches
        {
            get
            {
                var matchingService = ServiceLocator.Get<IClueMatchingService>();
                return matchingService?.CorrectMatchCount ?? 0;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeStateService();
        }

        private void Start()
        {
            _stateService?.Initialize(GameState.IntroVideo);
        }

        private void InitializeStateService()
        {
            _stateService = ServiceLocator.Get<IGameStateService>();
            if (_stateService == null)
            {
                _stateService = new GameStateService();
                ServiceLocator.Register<IGameStateService>(_stateService);
            }
        }

        public void SetState(GameState newState)
        {
            _stateService?.TransitionTo(newState);
            LoggingUtility.LogState($"Requested state transition to: {newState}");
        }

        /// <summary>
        /// Record or update a verdict for a suspect using data from IRoleAssignmentService.
        /// </summary>
        public void RecordVerdict(SuspectData suspect, SuspectRole playerChoice)
        {
            _verdictRecords.RemoveAll(r => r.Suspect == suspect);
            var record = new VerdictRecord(suspect, playerChoice);
            _verdictRecords.Add(record);
        }

        /// <summary>
        /// Rebuild verdict records from IRoleAssignmentService data.
        /// Called before transitioning to Results.
        /// </summary>
        public void RebuildVerdictRecordsFromService()
        {
            var roleService = ServiceLocator.Get<IRoleAssignmentService>();
            if (roleService == null || roleService.Suspects == null) return;

            _verdictRecords.Clear();
            foreach (var suspect in roleService.Suspects)
            {
                var role = roleService.GetAssignedRole(suspect);
                if (role.HasValue)
                {
                    _verdictRecords.Add(new VerdictRecord(suspect, role.Value));
                }
            }
        }

        public void AdvanceToNextLevel(Action onComplete = null)
        {
            if (_isTransitioning)
            {
                LoggingUtility.Warning("GameManager", "Already transitioning levels!");
                return;
            }

            _currentLevelIndex++;

            if (_currentLevelIndex >= _levels.Count)
            {
                LoggingUtility.LogState("All levels completed!");
                _currentLevelIndex = _levels.Count - 1;
                onComplete?.Invoke();
                return;
            }

            PerformLevelTransition(onComplete);
        }

        public void RestartCurrentLevel(Action onComplete = null)
        {
            if (_isTransitioning)
            {
                LoggingUtility.Warning("GameManager", "Already transitioning!");
                return;
            }

            PerformLevelTransition(onComplete);
        }

        public void LoadLevel(int levelIndex, Action onComplete = null)
        {
            if (_isTransitioning)
            {
                LoggingUtility.Warning("GameManager", "Already transitioning!");
                return;
            }

            if (levelIndex < 0 || levelIndex >= _levels.Count)
            {
                LoggingUtility.Error("GameManager", $"Invalid level index: {levelIndex}");
                return;
            }

            _currentLevelIndex = levelIndex;
            PerformLevelTransition(onComplete);
        }

        private void PerformLevelTransition(Action onComplete)
        {
            _isTransitioning = true;
            _verdictRecords.Clear();

            var matchingService = ServiceLocator.Get<IClueMatchingService>();
            matchingService?.Clear();

            var roleService = ServiceLocator.Get<IRoleAssignmentService>();
            roleService?.Clear();

            if (_fadeTransition != null)
            {
                _fadeTransition.FadeInOut(
                    onMiddle: () =>
                    {
                        UIManager.Instance?.HideAllPanels();
                        
                        if (LevelManager.Instance != null && CurrentLevel != null)
                        {
                            LevelManager.Instance.LoadLevel(CurrentLevel);
                        }
                        SetState(GameState.ClueSearch);
                    },
                    onComplete: () =>
                    {
                        _isTransitioning = false;
                        onComplete?.Invoke();
                        LoggingUtility.LogState($"Transition to Level {_currentLevelIndex + 1} complete!");
                    }
                );
            }
            else
            {
                UIManager.Instance?.HideAllPanels();
                
                if (LevelManager.Instance != null && CurrentLevel != null)
                {
                    LevelManager.Instance.LoadLevel(CurrentLevel);
                }
                SetState(GameState.ClueSearch);
                _isTransitioning = false;
                onComplete?.Invoke();
            }
        }

        public bool HasNextLevel()
        {
            return _currentLevelIndex < _levels.Count - 1;
        }

        public void OnLevelComplete()
        {
            if (HasNextLevel())
            {
                AdvanceToNextLevel();
            }
            else
            {
                LoggingUtility.LogState("Game Complete! All levels finished.");
            }
        }

        private void OnValidate()
        {
            if (_levels == null || _levels.Count == 0)
            {
                LoggingUtility.Warning("GameManager", "No levels assigned in inspector.");
            }
        }
    }
}