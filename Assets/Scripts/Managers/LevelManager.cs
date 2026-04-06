using UnityEngine;
using CriminalCase2.Data;
using System.Collections.Generic;

namespace CriminalCase2.Managers
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [SerializeField] private LevelConfig _levelConfig;

        private List<SuspectData> _judgedSuspects = new List<SuspectData>();
        private int _drugTestsRemaining;

        public LevelConfig LevelConfig => _levelConfig;
        public int DrugTestsRemaining => _drugTestsRemaining;
        public bool AllSuspectsJudged => _judgedSuspects.Count >= _levelConfig.Suspects.Length;
        public int JudgedCount => _judgedSuspects.Count;
        public int TotalSuspects => _levelConfig.Suspects.Length;

        public bool IsSuspectJudged(SuspectData suspect)
        {
            return _judgedSuspects.Contains(suspect);
        }

        public SuspectRole GetSuspectVerdict(SuspectData suspect)
        {
            foreach (var record in GameManager.Instance.VerdictRecords)
            {
                if (record.Suspect == suspect)
                {
                    return record.PlayerChoice;
                }
            }
            return SuspectRole.Normal;
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

        public void Initialize(LevelConfig config)
        {
            _levelConfig = config;
            _judgedSuspects.Clear();
            _drugTestsRemaining = config.MaxDrugTestsPerLevel;
            Debug.Log($"[LevelManager] Initialized level: {config.LevelName}");
        }

        public void RecordJudgedSuspect(SuspectData suspect, SuspectRole playerChoice)
        {
            if (!_judgedSuspects.Contains(suspect))
            {
                _judgedSuspects.Add(suspect);
                GameManager.Instance.RecordVerdict(suspect, playerChoice);

                if (AllSuspectsJudged)
                {
                    OnAllSuspectsJudged();
                }
            }
        }

        public bool UseDrugTest()
        {
            if (_drugTestsRemaining > 0)
            {
                _drugTestsRemaining--;
                return true;
            }

            Debug.Log("[LevelManager] No drug tests remaining.");
            return false;
        }

        private void OnAllSuspectsJudged()
        {
            Debug.Log("[LevelManager] All suspects judged. Waiting for player to check results.");
        }

        private void OnValidate()
        {
            if (_levelConfig == null)
            {
                Debug.LogWarning("[LevelManager] No LevelConfig assigned in inspector.");
            }
        }
    }
}
