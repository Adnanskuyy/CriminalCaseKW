using System;
using System.Collections.Generic;
using CriminalCase2.Data;

namespace CriminalCase2.Services.Interfaces
{
    /// <summary>
    /// Service interface for clue management functionality.
    /// Handles clue discovery, tracking, and querying.
    /// </summary>
    public interface IClueService
    {
        /// <summary>
        /// Event fired when a clue is discovered.
        /// </summary>
        event Action<ClueData> OnClueFound;

        /// <summary>
        /// Event fired when all clues in the current level are found.
        /// </summary>
        event Action OnAllCluesFound;

        /// <summary>
        /// Event fired when clues are initialized for a new level.
        /// </summary>
        event Action<ClueData[]> OnCluesInitialized;

        /// <summary>
        /// List of all found clues.
        /// </summary>
        IReadOnlyList<ClueData> FoundClues { get; }

        /// <summary>
        /// All clues for the current level.
        /// </summary>
        IReadOnlyList<ClueData> AllClues { get; }

        /// <summary>
        /// Number of clues found so far.
        /// </summary>
        int FoundCount { get; }

        /// <summary>
        /// Total number of clues in the current level.
        /// </summary>
        int TotalCount { get; }

        /// <summary>
        /// Whether all clues have been found.
        /// </summary>
        bool AllCluesFound { get; }

        /// <summary>
        /// Initialize the service with clues for a new level.
        /// </summary>
        void Initialize(ClueData[] clues);

        /// <summary>
        /// Register a clue as found.
        /// </summary>
        void FindClue(ClueData clue);

        /// <summary>
        /// Check if a specific clue has been found.
        /// </summary>
        bool IsClueFound(ClueData clue);

        /// <summary>
        /// Check if a specific clue has been found by name.
        /// </summary>
        bool IsClueFound(string clueName);

        /// <summary>
        /// Get a clue by name.
        /// </summary>
        ClueData GetClueByName(string clueName);

        /// <summary>
        /// Reset all found clues for the current level.
        /// </summary>
        void ResetFoundClues();

        /// <summary>
        /// Clear all clues and reset the service.
        /// </summary>
        void Clear();
    }
}
