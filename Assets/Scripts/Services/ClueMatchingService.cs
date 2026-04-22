using System;
using System.Collections.Generic;
using System.Linq;
using CriminalCase2.Data;
using CriminalCase2.Services.Interfaces;

namespace CriminalCase2.Services
{
    public class ClueMatchingService : IClueMatchingService
    {
        private ClueData[] _allClues;
        private SuspectData[] _suspects;
        private Dictionary<ClueData, SuspectData> _assignments = new Dictionary<ClueData, SuspectData>();
        private bool _confirmed;

        public event Action<ClueData, SuspectData> OnClueAssigned;
        public event Action<ClueData> OnClueUnassigned;
        public event Action OnAllCluesAssigned;
        public event Action OnMatchingConfirmed;

        public IReadOnlyList<ClueData> AllClues => _allClues;
        public bool AllCluesAssigned => _allClues != null && _assignments.Count == _allClues.Length;
        public bool IsConfirmed => _confirmed;
        public int TotalClueCount => _allClues?.Length ?? 0;

        public IReadOnlyList<ClueData> UnassignedClues
        {
            get
            {
                if (_allClues == null) return new List<ClueData>();
                return _allClues.Where(c => !_assignments.ContainsKey(c)).ToList().AsReadOnly();
            }
        }

        public int CorrectMatchCount
        {
            get
            {
                if (_allClues == null || _suspects == null) return 0;
                int correct = 0;
                foreach (var kvp in _assignments)
                {
                    if (kvp.Key.LinkedSuspect != null && kvp.Key.LinkedSuspect == kvp.Value)
                        correct++;
                }
                return correct;
            }
        }

        public float MatchingAccuracy
        {
            get
            {
                if (_allClues == null || _allClues.Length == 0) return 0f;
                return (float)CorrectMatchCount / _allClues.Length;
            }
        }

        public void Initialize(ClueData[] clues, SuspectData[] suspects)
        {
            _allClues = clues ?? Array.Empty<ClueData>();
            _suspects = suspects ?? Array.Empty<SuspectData>();
            _assignments.Clear();
            _confirmed = false;
        }

        public void AssignClue(ClueData clue, SuspectData suspect)
        {
            if (clue == null || suspect == null) return;
            if (_allClues == null || !_allClues.Contains(clue)) return;
            if (!_suspects.Contains(suspect)) return;

            _assignments[clue] = suspect;
            OnClueAssigned?.Invoke(clue, suspect);

            if (AllCluesAssigned)
                OnAllCluesAssigned?.Invoke();
        }

        public void UnassignClue(ClueData clue)
        {
            if (clue == null || !_assignments.ContainsKey(clue)) return;

            _assignments.Remove(clue);
            OnClueUnassigned?.Invoke(clue);
        }

        public bool IsClueAssigned(ClueData clue)
        {
            return clue != null && _assignments.ContainsKey(clue);
        }

        public SuspectData GetAssignedSuspect(ClueData clue)
        {
            if (clue != null && _assignments.TryGetValue(clue, out var suspect))
                return suspect;
            return null;
        }

        public IReadOnlyList<ClueData> GetCluesForSuspect(SuspectData suspect)
        {
            if (suspect == null) return new List<ClueData>();
            return _assignments
                .Where(kvp => kvp.Value == suspect)
                .Select(kvp => kvp.Key)
                .ToList()
                .AsReadOnly();
        }

        public void ConfirmMatchings()
        {
            if (!AllCluesAssigned) return;
            _confirmed = true;
            OnMatchingConfirmed?.Invoke();
        }

        public void Clear()
        {
            _allClues = null;
            _suspects = null;
            _assignments.Clear();
            _confirmed = false;
        }
    }
}
