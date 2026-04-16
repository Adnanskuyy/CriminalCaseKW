using System;
using System.Collections.Generic;
using CriminalCase2.Data;
using CriminalCase2.Services;
using CriminalCase2.Services.Interfaces;
using CriminalCase2.Utils;
using UnityEngine;

namespace CriminalCase2.Managers
{
    /// <summary>
    /// Manager class for clue functionality.
    /// Acts as a facade to the ClueService for backward compatibility.
    /// New code should use IClueService directly via ServiceLocator.
    /// </summary>
    public class ClueManager : MonoBehaviour
    {
        public static ClueManager Instance { get; private set; }

        private IClueService _clueService;

        // Legacy events - forwarded from ClueService
        public event Action<ClueData> OnClueFoundEvent
        {
            add
            {
                if (_clueService != null)
                    _clueService.OnClueFound += value;
            }
            remove
            {
                if (_clueService != null)
                    _clueService.OnClueFound -= value;
            }
        }

        public event Action OnAllCluesFoundEvent
        {
            add
            {
                if (_clueService != null)
                    _clueService.OnAllCluesFound += value;
            }
            remove
            {
                if (_clueService != null)
                    _clueService.OnAllCluesFound -= value;
            }
        }

        // Legacy properties - delegated to ClueService
        public IReadOnlyList<ClueData> FoundClues => _clueService?.FoundClues;
        public int FoundCount => _clueService?.FoundCount ?? 0;
        public int TotalClueCount => _clueService?.TotalCount ?? 0;
        public bool AllCluesFound => _clueService?.AllCluesFound ?? false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Get or create ClueService
            _clueService = ServiceLocator.Get<IClueService>();
            if (_clueService == null)
            {
                _clueService = new ClueService();
                ServiceLocator.Register<IClueService>(_clueService);
                LoggingUtility.LogClue("ClueService registered with ServiceLocator");
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Initialize with level clues. Legacy method - delegates to ClueService.
        /// </summary>
        public void Initialize(ClueData[] clues)
        {
            _clueService?.Initialize(clues);
        }

        /// <summary>
        /// Register a clue as found. Legacy method - delegates to ClueService.
        /// </summary>
        public void OnClueFound(ClueData clue)
        {
            _clueService?.FindClue(clue);
        }

        /// <summary>
        /// Check if a clue has been found. Legacy method - delegates to ClueService.
        /// </summary>
        public bool IsClueFound(ClueData clue)
        {
            return _clueService?.IsClueFound(clue) ?? false;
        }

        /// <summary>
        /// Reset all clues. Legacy method - delegates to ClueService.
        /// </summary>
        public void ResetClues()
        {
            _clueService?.Clear();
        }
    }
}
