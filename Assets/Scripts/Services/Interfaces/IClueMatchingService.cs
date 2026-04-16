using System;
using System.Collections.Generic;
using CriminalCase2.Data;

namespace CriminalCase2.Services.Interfaces
{
    public interface IClueMatchingService
    {
        event Action<ClueData, SuspectData> OnClueAssigned;
        event Action<ClueData> OnClueUnassigned;
        event Action OnAllCluesAssigned;
        event Action OnMatchingConfirmed;

        IReadOnlyList<ClueData> AllClues { get; }
        IReadOnlyList<ClueData> UnassignedClues { get; }
        bool AllCluesAssigned { get; }
        bool IsConfirmed { get; }
        int CorrectMatchCount { get; }
        int TotalClueCount { get; }
        float MatchingAccuracy { get; }

        void Initialize(ClueData[] clues, SuspectData[] suspects);
        void AssignClue(ClueData clue, SuspectData suspect);
        void UnassignClue(ClueData clue);
        bool IsClueAssigned(ClueData clue);
        SuspectData GetAssignedSuspect(ClueData clue);
        IReadOnlyList<ClueData> GetCluesForSuspect(SuspectData suspect);
        void ConfirmMatchings();
        void Clear();
    }
}
