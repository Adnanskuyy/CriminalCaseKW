using UnityEngine;
using UnityEngine.Video;
using CriminalCase2.Data;
using System.Collections.Generic;

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

        private GameState _currentState;
        private List<VerdictRecord> _verdictRecords = new List<VerdictRecord>();

        public GameState CurrentState => _currentState;
        public LevelConfig CurrentLevel => _currentLevelIndex < _levels.Count ? _levels[_currentLevelIndex] : null;
        public int CurrentLevelIndex => _currentLevelIndex;
        public IReadOnlyList<VerdictRecord> VerdictRecords => _verdictRecords.AsReadOnly();
        public int TotalLevels => _levels.Count;
        public VideoClip GlobalIntroVideo => _globalIntroVideo;

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
            Debug.Log($"[GameManager] State changed to: {newState}");
        }

        public void RecordVerdict(SuspectData suspect, SuspectRole playerChoice)
        {
            var record = new VerdictRecord(suspect, playerChoice);
            _verdictRecords.Add(record);
        }

        public void AdvanceToNextLevel()
        {
            _currentLevelIndex++;
            _verdictRecords.Clear();

            if (_currentLevelIndex >= _levels.Count)
            {
                Debug.Log("[GameManager] All levels completed!");
                _currentLevelIndex = _levels.Count - 1;
            }
        }

        private void OnValidate()
        {
            if (_levels == null || _levels.Count == 0)
            {
                Debug.LogWarning("[GameManager] No levels assigned in inspector.");
            }
        }
    }
}
