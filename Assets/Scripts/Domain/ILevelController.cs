using System;
using CriminalCase2.Data;

namespace CriminalCase2.Domain
{
    public interface ILevelController
    {
        LevelConfig? CurrentLevelConfig { get; }
        int JudgedCount { get; }
        int TotalSuspects { get; }
        int DrugTestsRemaining { get; }
        bool AllSuspectsJudged { get; }

        bool IsSuspectJudged(SuspectData suspect);
        SuspectRole? GetSuspectVerdict(SuspectData suspect);
        void RecordJudgedSuspect(SuspectData suspect, SuspectRole playerChoice);

        bool UseDrugTest();
        void RecordDrugTest(SuspectData suspect, DrugTestResult result);
        bool HasDrugTestResult(SuspectData suspect);
        DrugTestResult GetDrugTestResult(SuspectData suspect);

        void LoadLevel(LevelConfig config);

        event Action<LevelConfig>? LevelLoaded;
    }
}
