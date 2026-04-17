using System;
using System.Collections.Generic;
using CriminalCase2.Data;

namespace CriminalCase2.Services.Interfaces
{
    public interface IRoleAssignmentService
    {
        event Action<SuspectData, SuspectRole> OnRoleAssigned;
        event Action<SuspectData, SuspectRole> OnRoleChanged;
        event Action OnAllRolesAssigned;
        event Action<SuspectData, DrugTestResult> OnDrugTestUsed;

        IReadOnlyList<SuspectData> Suspects { get; }
        int DrugTestsRemaining { get; }
        int MaxDrugTests { get; }
        int DrugTestsUsed { get; }
        bool AllRolesAssigned { get; }
        int AssignedCount { get; }
        int TotalSuspects { get; }
        bool IsInitialized { get; }

        bool HasDrugTestResult(SuspectData suspect);
        DrugTestResult GetDrugTestResult(SuspectData suspect);
        SuspectRole? GetAssignedRole(SuspectData suspect);
        bool IsSuspectAssigned(SuspectData suspect);

        bool UseDrugTest(SuspectData suspect);
        void AssignRole(SuspectData suspect, SuspectRole role);
        void UnassignRole(SuspectData suspect);
        void Initialize(SuspectData[] suspects, int maxDrugTests);
        void Clear();
    }
}