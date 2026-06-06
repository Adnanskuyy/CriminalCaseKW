using UnityEngine;
using CriminalCase2.Data;
using CriminalCase2.Domain;
using CriminalCase2.Services;
using CriminalCase2.Utils;
using System;
using System.Collections.Generic;

namespace CriminalCase2.Managers
{
    public class LevelManager : MonoBehaviour, ILevelController
    {
        [Header("Level Setup")]
        [SerializeField] private Transform _levelSpawnPoint;

        private LevelConfig _currentLevelConfig;
        private GameObject _currentLevelInstance;
        private List<SuspectData> _judgedSuspects = new List<SuspectData>();
        private Dictionary<SuspectData, DrugTestResult> _drugTestResults = new Dictionary<SuspectData, DrugTestResult>();
        private int _drugTestsRemaining;

        public LevelConfig? CurrentLevelConfig => _currentLevelConfig;
        public GameObject CurrentLevelInstance => _currentLevelInstance;
        public int DrugTestsRemaining => _drugTestsRemaining;
        public bool AllSuspectsJudged => _judgedSuspects.Count >= (_currentLevelConfig?.Suspects.Length ?? 0);
        public int JudgedCount => _judgedSuspects.Count;
        public int TotalSuspects => _currentLevelConfig?.Suspects.Length ?? 0;

        public event Action<LevelConfig>? LevelLoaded;

        public bool IsSuspectJudged(SuspectData suspect)
        {
            return _judgedSuspects.Contains(suspect);
        }

        public SuspectRole? GetSuspectVerdict(SuspectData suspect)
        {
            var verdicts = GameServices.Verdicts;
            if (verdicts == null) return null;

            foreach (var record in verdicts.Records)
            {
                if (record.Suspect == suspect)
                {
                    return record.PlayerChoice;
                }
            }
            return null;
        }

        private void Start()
        {
            // Load initial level from game session
            var session = GameServices.GameState;
            if (session != null && session.CurrentLevel != null)
            {
                LoadLevel(session.CurrentLevel);
            }
        }

        /// <summary>
        /// Load a level by its config
        /// </summary>
        public void LoadLevel(LevelConfig config)
        {
            if (config == null)
            {
                GameLogger.Error("[LevelManager] Cannot load null level config!");
                return;
            }

            // Unload current level if any
            UnloadCurrentLevel();

            _currentLevelConfig = config;

            // Spawn level prefab
            if (config.LevelPrefab != null)
            {
                Vector3 spawnPosition = _levelSpawnPoint != null ? _levelSpawnPoint.position : Vector3.zero;
                _currentLevelInstance = Instantiate(config.LevelPrefab, spawnPosition, Quaternion.identity);
                _currentLevelInstance.name = $"Level_{config.LevelIndex:D2}_Instance";
                GameLogger.Info($"[LevelManager] Spawned level prefab: {_currentLevelInstance.name}");
            }
            else
            {
                GameLogger.Warn($"[LevelManager] No prefab assigned for level: {config.LevelName}");
            }

            // Initialize level data
            Initialize(config);

            GameLogger.Info($"[LevelManager] Loaded level: {config.LevelName}");
            LevelLoaded?.Invoke(config);
        }

        /// <summary>
        /// Unload the current level
        /// </summary>
        public void UnloadCurrentLevel()
        {
            if (_currentLevelInstance != null)
            {
                GameLogger.Info($"[LevelManager] Unloading level: {_currentLevelInstance.name}");
                Destroy(_currentLevelInstance);
                _currentLevelInstance = null;
            }

            _judgedSuspects.Clear();
            _drugTestResults.Clear();
            _currentLevelConfig = null;
        }

        public void Initialize(LevelConfig config)
        {
            _judgedSuspects.Clear();
            _drugTestResults.Clear();
            _drugTestsRemaining = config.MaxDrugTestsPerLevel;
            GameLogger.Info($"[LevelManager] Initialized level: {config.LevelName}");
        }

        public void RecordJudgedSuspect(SuspectData suspect, SuspectRole playerChoice)
        {
            bool isNew = !_judgedSuspects.Contains(suspect);
            if (isNew)
            {
                _judgedSuspects.Add(suspect);
            }
            GameServices.Verdicts?.Record(suspect, playerChoice);

            if (AllSuspectsJudged)
            {
                OnAllSuspectsJudged();
            }
        }

        public bool UseDrugTest()
        {
            if (_drugTestsRemaining > 0)
            {
                _drugTestsRemaining--;
                return true;
            }

            GameLogger.Info("[LevelManager] No drug tests remaining.");
            return false;
        }

        public void RecordDrugTest(SuspectData suspect, DrugTestResult result)
        {
            if (suspect == null) return;
            _drugTestResults[suspect] = result;
        }

        public bool HasDrugTestResult(SuspectData suspect)
        {
            return suspect != null && _drugTestResults.ContainsKey(suspect);
        }

        public DrugTestResult GetDrugTestResult(SuspectData suspect)
        {
            if (suspect != null && _drugTestResults.TryGetValue(suspect, out var result))
                return result;
            return DrugTestResult.Negative;
        }

        private void OnAllSuspectsJudged()
        {
            GameLogger.Info("[LevelManager] All suspects judged. Waiting for player to check results.");
        }
    }
}
