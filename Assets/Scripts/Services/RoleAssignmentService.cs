using System;
using System.Collections.Generic;
using System.Linq;
using CriminalCase2.Data;
using CriminalCase2.Services.Interfaces;
using CriminalCase2.Utils;

namespace CriminalCase2.Services
{
    public class RoleAssignmentService : IRoleAssignmentService
    {
        private SuspectData[] _suspects;
        private readonly Dictionary<SuspectData, SuspectRole> _assignedRoles = new Dictionary<SuspectData, SuspectRole>();
        private readonly Dictionary<SuspectData, DrugTestResult> _drugTestResults = new Dictionary<SuspectData, DrugTestResult>();
        private int _drugTestsRemaining;
        private int _maxDrugTests;

        public event Action<SuspectData, SuspectRole> OnRoleAssigned;
        public event Action<SuspectData, SuspectRole> OnRoleChanged;
        public event Action OnAllRolesAssigned;
        public event Action<SuspectData, DrugTestResult> OnDrugTestUsed;

        public IReadOnlyList<SuspectData> Suspects => _suspects != null ? Array.AsReadOnly(_suspects) : null;
        public int DrugTestsRemaining => _drugTestsRemaining;
        public int MaxDrugTests => _maxDrugTests;
        public int DrugTestsUsed => _maxDrugTests - _drugTestsRemaining;
        public bool AllRolesAssigned => _suspects != null && _assignedRoles.Count == _suspects.Length;
        public int AssignedCount => _assignedRoles.Count;
        public int TotalSuspects => _suspects?.Length ?? 0;
        public bool IsInitialized => _suspects != null;

        public bool HasDrugTestResult(SuspectData suspect)
        {
            return suspect != null && _drugTestResults.ContainsKey(suspect);
        }

        public DrugTestResult GetDrugTestResult(SuspectData suspect)
        {
            if (suspect != null && _drugTestResults.TryGetValue(suspect, out var result))
                return result;
            return DrugTestResult.Negative;
        }

        public SuspectRole? GetAssignedRole(SuspectData suspect)
        {
            if (suspect != null && _assignedRoles.TryGetValue(suspect, out var role))
                return role;
            return null;
        }

        public bool IsSuspectAssigned(SuspectData suspect)
        {
            return suspect != null && _assignedRoles.ContainsKey(suspect);
        }

        public bool UseDrugTest(SuspectData suspect)
        {
            if (suspect == null)
            {
                LoggingUtility.Warning("RoleAssignment", "Cannot use drug test: suspect is null.");
                return false;
            }

            if (_drugTestsRemaining <= 0)
            {
                LoggingUtility.Warning("RoleAssignment", "No drug tests remaining.");
                return false;
            }

            if (_drugTestResults.ContainsKey(suspect))
            {
                LoggingUtility.Warning("RoleAssignment", $"Drug test already used on {suspect.SuspectName}.");
                return false;
            }

            _drugTestsRemaining--;
            var result = suspect.DrugTestResult;
            _drugTestResults[suspect] = result;

            LoggingUtility.LogDebug("RoleAssignment", $"Drug test used on {suspect.SuspectName}: {result.ToDisplayName()} (Remaining: {_drugTestsRemaining})");

            OnDrugTestUsed?.Invoke(suspect, result);
            return true;
        }

        public void AssignRole(SuspectData suspect, SuspectRole role)
        {
            if (suspect == null)
            {
                LoggingUtility.Warning("RoleAssignment", "Cannot assign role: suspect is null.");
                return;
            }

            if (_suspects == null || !_suspects.Contains(suspect))
            {
                LoggingUtility.Warning("RoleAssignment", $"Suspect {suspect.SuspectName} is not part of current level.");
                return;
            }

            bool wasAlreadyAssigned = _assignedRoles.ContainsKey(suspect);
            var previousRole = wasAlreadyAssigned ? _assignedRoles[suspect] : (SuspectRole?)null;

            _assignedRoles[suspect] = role;

            if (wasAlreadyAssigned && previousRole != role)
            {
                LoggingUtility.LogDebug("RoleAssignment", $"Role changed for {suspect.SuspectName}: {previousRole.Value.ToDisplayName()} -> {role.ToDisplayName()}");
                OnRoleChanged?.Invoke(suspect, role);
            }
            else if (!wasAlreadyAssigned)
            {
                LoggingUtility.LogDebug("RoleAssignment", $"Role assigned: {suspect.SuspectName} -> {role.ToDisplayName()}");
                OnRoleAssigned?.Invoke(suspect, role);
            }

            if (AllRolesAssigned)
            {
                LoggingUtility.LogDebug("RoleAssignment", "All suspects have been assigned roles.");
                OnAllRolesAssigned?.Invoke();
            }
        }

        public void UnassignRole(SuspectData suspect)
        {
            if (suspect == null || !_assignedRoles.ContainsKey(suspect)) return;

            _assignedRoles.Remove(suspect);
            LoggingUtility.LogDebug("RoleAssignment", $"Role unassigned for {suspect.SuspectName}");
        }

        public void Initialize(SuspectData[] suspects, int maxDrugTests)
        {
            _suspects = suspects ?? Array.Empty<SuspectData>();
            _maxDrugTests = maxDrugTests;
            _drugTestsRemaining = maxDrugTests;
            _assignedRoles.Clear();
            _drugTestResults.Clear();

            LoggingUtility.LogDebug("RoleAssignment", $"Initialized with {_suspects.Length} suspects and {maxDrugTests} drug tests.");
        }

        public void Clear()
        {
            _suspects = null;
            _assignedRoles.Clear();
            _drugTestResults.Clear();
            _drugTestsRemaining = 0;
            _maxDrugTests = 0;

            LoggingUtility.LogDebug("RoleAssignment", "Service cleared.");
        }
    }
}