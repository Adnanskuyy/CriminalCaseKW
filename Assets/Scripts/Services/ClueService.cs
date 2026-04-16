using System;
using System.Collections.Generic;
using System.Linq;
using CriminalCase2.Data;
using CriminalCase2.Services.Interfaces;
using CriminalCase2.Utils;

namespace CriminalCase2.Services
{
    /// <summary>
    /// Service implementation for clue management.
    /// Handles clue discovery, tracking, and querying.
    /// </summary>
    public class ClueService : IClueService
    {
        private readonly List<ClueData> _foundClues = new();
        private readonly List<ClueData> _allClues = new();

        #region Events

        public event Action<ClueData> OnClueFound;
        public event Action OnAllCluesFound;
        public event Action<ClueData[]> OnCluesInitialized;

        #endregion

        #region Properties

        public IReadOnlyList<ClueData> FoundClues => _foundClues.AsReadOnly();
        public IReadOnlyList<ClueData> AllClues => _allClues.AsReadOnly();
        public int FoundCount => _foundClues.Count;
        public int TotalCount => _allClues.Count;
        public bool AllCluesFound => _foundClues.Count >= _allClues.Count && _allClues.Count > 0;

        #endregion

        #region IClueService Implementation

        public void Initialize(ClueData[] clues)
        {
            _foundClues.Clear();
            _allClues.Clear();

            if (clues != null && clues.Length > 0)
            {
                _allClues.AddRange(clues);
                LoggingUtility.LogClue($"Initialized with {clues.Length} clues");
            }
            else
            {
                LoggingUtility.Warning("Clue", "Initialized with no clues");
            }

            OnCluesInitialized?.Invoke(clues);
        }

        public void FindClue(ClueData clue)
        {
            if (clue == null)
            {
                LoggingUtility.Warning("Clue", "Cannot find null clue");
                return;
            }

            if (_foundClues.Contains(clue))
            {
                LoggingUtility.LogDebug("Clue", $"Clue '{clue.ClueName}' already found, ignoring");
                return;
            }

            _foundClues.Add(clue);
            LoggingUtility.LogClue($"Found clue: {clue.ClueName} ({_foundClues.Count}/{_allClues.Count})");

            // Notify listeners
            OnClueFound?.Invoke(clue);

            // Check if all clues found
            if (AllCluesFound)
            {
                LoggingUtility.LogClue("All clues found!");
                OnAllCluesFound?.Invoke();
            }
        }

        public bool IsClueFound(ClueData clue)
        {
            return clue != null && _foundClues.Contains(clue);
        }

        public bool IsClueFound(string clueName)
        {
            if (string.IsNullOrEmpty(clueName)) return false;
            return _foundClues.Any(c => c.ClueName == clueName);
        }

        public ClueData GetClueByName(string clueName)
        {
            if (string.IsNullOrEmpty(clueName)) return null;
            return _allClues.FirstOrDefault(c => c.ClueName == clueName);
        }

        public void ResetFoundClues()
        {
            int count = _foundClues.Count;
            _foundClues.Clear();
            LoggingUtility.LogClue($"Reset {count} found clues");
        }

        public void Clear()
        {
            _foundClues.Clear();
            _allClues.Clear();
            LoggingUtility.LogClue("Service cleared");
        }

        #endregion
    }
}
