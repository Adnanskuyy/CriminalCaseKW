using System;
using System.Collections.Generic;
using CriminalCase2.Data;
using UnityEngine;

namespace CriminalCase2.Managers
{
    public class ClueManager : MonoBehaviour
    {
        public static ClueManager Instance { get; private set; }

        private List<ClueData> _foundClues = new List<ClueData>();
        private ClueData[] _levelClues;
        private int _totalClueCount;

        public IReadOnlyList<ClueData> FoundClues => _foundClues.AsReadOnly();
        public int FoundCount => _foundClues.Count;
        public int TotalClueCount => _totalClueCount;
        public bool AllCluesFound => _foundClues.Count >= _totalClueCount && _totalClueCount > 0;

        public event Action<ClueData> OnClueFoundEvent;
        public event Action OnAllCluesFoundEvent;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void Initialize(ClueData[] clues)
        {
            _levelClues = clues;
            _totalClueCount = clues != null ? clues.Length : 0;
            _foundClues.Clear();
            Debug.Log($"[ClueManager] Initialized with {_totalClueCount} clues.");
        }

        public void OnClueFound(ClueData clue)
        {
            if (clue == null) return;
            if (_foundClues.Contains(clue)) return;

            _foundClues.Add(clue);
            Debug.Log($"[ClueManager] Clue found: {clue.ClueName} ({_foundClues.Count}/{_totalClueCount})");

            if (clue.IsDrugTestClue)
            {
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.AddBonusDrugTest();
                }
            }

            OnClueFoundEvent?.Invoke(clue);

            if (AllCluesFound)
            {
                Debug.Log("[ClueManager] All clues found!");
                OnAllCluesFoundEvent?.Invoke();
            }
        }

        public bool IsClueFound(ClueData clue)
        {
            return clue != null && _foundClues.Contains(clue);
        }

        public void ResetClues()
        {
            _foundClues.Clear();
            _levelClues = null;
            _totalClueCount = 0;
        }
    }
}