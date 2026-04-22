using UnityEngine;
using CriminalCase2.Data;
using CriminalCase2.Interactables;
using CriminalCase2.Services;
using CriminalCase2.Services.Interfaces;
using CriminalCase2.Utils;
using System.Collections.Generic;

namespace CriminalCase2.Managers
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Level Setup")]
        [SerializeField] private Transform _levelSpawnPoint;
        
        private LevelConfig _currentLevelConfig;
        private GameObject _currentLevelInstance;

        public LevelConfig CurrentLevelConfig => _currentLevelConfig;
        public GameObject CurrentLevelInstance => _currentLevelInstance;

        public int DrugTestsRemaining
        {
            get
            {
                var roleService = ServiceLocator.Get<IRoleAssignmentService>();
                return roleService?.DrugTestsRemaining ?? 0;
            }
        }

        public bool AllSuspectsJudged
        {
            get
            {
                var roleService = ServiceLocator.Get<IRoleAssignmentService>();
                return roleService?.AllRolesAssigned ?? false;
            }
        }

        public int JudgedCount
        {
            get
            {
                var roleService = ServiceLocator.Get<IRoleAssignmentService>();
                return roleService?.AssignedCount ?? 0;
            }
        }

        public int TotalSuspects
        {
            get
            {
                var roleService = ServiceLocator.Get<IRoleAssignmentService>();
                return roleService?.TotalSuspects ?? (_currentLevelConfig?.Suspects.Length ?? 0);
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
        }

        private void Start()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentLevel != null)
            {
                LoadLevel(GameManager.Instance.CurrentLevel);
            }
        }

        public void LoadLevel(LevelConfig config)
        {
            if (config == null)
            {
                Debug.LogError("[LevelManager] Cannot load null level config!");
                return;
            }

            UnloadCurrentLevel();

            _currentLevelConfig = config;
            
            if (config.LevelPrefab != null)
            {
                Vector3 spawnPosition = _levelSpawnPoint != null ? _levelSpawnPoint.position : Vector3.zero;
                _currentLevelInstance = Instantiate(config.LevelPrefab, spawnPosition, Quaternion.identity);
                _currentLevelInstance.name = $"Level_{config.LevelIndex:D2}_Instance";
                Debug.Log($"[LevelManager] Spawned level prefab: {_currentLevelInstance.name}");
            }
            else
            {
                Debug.LogWarning($"[LevelManager] No prefab assigned for level: {config.LevelName}");
            }

            Initialize(config);
            
            Debug.Log($"[LevelManager] Loaded level: {config.LevelName}");
        }

        public void UnloadCurrentLevel()
        {
            if (_currentLevelInstance != null)
            {
                Debug.Log($"[LevelManager] Unloading level: {_currentLevelInstance.name}");
                Destroy(_currentLevelInstance);
                _currentLevelInstance = null;
            }

            var roleService = ServiceLocator.Get<IRoleAssignmentService>();
            roleService?.Clear();
            _currentLevelConfig = null;
        }

        public void Initialize(LevelConfig config)
        {
            var roleService = ServiceLocator.Get<IRoleAssignmentService>();
            if (roleService != null && config.Suspects != null)
            {
                roleService.Initialize(config.Suspects, config.MaxDrugTestsPerLevel);
            }

            if (ClueManager.Instance != null && config.Clues != null)
            {
                ClueManager.Instance.Initialize(config.Clues);
            }

            DeactivateSuspects();

            Debug.Log($"[LevelManager] Initialized level: {config.LevelName}");
        }

        private void DeactivateSuspects()
        {
            if (_currentLevelInstance == null) return;

            var clickHandlers = _currentLevelInstance.GetComponentsInChildren<SuspectClickHandler>();
            foreach (var handler in clickHandlers)
            {
                handler.gameObject.SetActive(false);
            }
        }

        public void RevealSuspects()
        {
            if (_currentLevelInstance == null) return;

            var clickHandlers = _currentLevelInstance.GetComponentsInChildren<SuspectClickHandler>();
            foreach (var handler in clickHandlers)
            {
                handler.gameObject.SetActive(true);
            }
        }

        public void RecordJudgedSuspect(SuspectData suspect, SuspectRole playerChoice)
        {
            var roleService = ServiceLocator.Get<IRoleAssignmentService>();
            if (roleService != null)
            {
                roleService.AssignRole(suspect, playerChoice);
            }

            GameManager.Instance?.RecordVerdict(suspect, playerChoice);
        }

        public bool IsSuspectJudged(SuspectData suspect)
        {
            var roleService = ServiceLocator.Get<IRoleAssignmentService>();
            return roleService?.IsSuspectAssigned(suspect) ?? false;
        }

        public SuspectRole GetSuspectVerdict(SuspectData suspect)
        {
            var roleService = ServiceLocator.Get<IRoleAssignmentService>();
            var role = roleService?.GetAssignedRole(suspect);
            return role ?? SuspectRole.Normal;
        }

        public bool UseDrugTest()
        {
            var roleService = ServiceLocator.Get<IRoleAssignmentService>();
            return roleService?.DrugTestsRemaining > 0;
        }

        public void RecordDrugTest(SuspectData suspect, DrugTestResult result)
        {
            var roleService = ServiceLocator.Get<IRoleAssignmentService>();
            roleService?.UseDrugTest(suspect);
        }

        public bool HasDrugTestResult(SuspectData suspect)
        {
            var roleService = ServiceLocator.Get<IRoleAssignmentService>();
            return roleService?.HasDrugTestResult(suspect) ?? false;
        }

        public DrugTestResult GetDrugTestResult(SuspectData suspect)
        {
            var roleService = ServiceLocator.Get<IRoleAssignmentService>();
            return roleService?.GetDrugTestResult(suspect) ?? DrugTestResult.Negative;
        }

        private void OnValidate()
        {
        }
    }
}