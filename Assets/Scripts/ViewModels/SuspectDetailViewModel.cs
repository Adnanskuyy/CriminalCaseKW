#nullable enable
using System;
using CriminalCase2.Data;
using CriminalCase2.Domain;

namespace CriminalCase2.ViewModels
{
    /// <summary>
    /// ViewModel for the suspect detail panel. Pure C# (no Unity types beyond
    /// the <see cref="SuspectData"/> ScriptableObject) so it can be exercised
    /// by EditMode tests without instantiating a <see cref="MonoBehaviour"/>.
    ///
    /// Owns the currently-displayed <see cref="SuspectData"/> and exposes the
    /// state the view needs to render plus the commands the user can invoke.
    /// UI orchestration (hide panels, show HUD, refresh HUD) stays in the view:
    /// the VM signals intent through <see cref="VerdictRecorded"/> and
    /// <see cref="CloseRequested"/> events and lets the view decide what to do.
    /// </summary>
    public sealed class SuspectDetailViewModel : IDisposable
    {
        private readonly SuspectData _suspect;
        private readonly ILevelController _levels;

        public SuspectData Suspect => _suspect;
        public string SuspectName => _suspect.SuspectName;
        public string Description => _suspect.Description;
        public string EvidenceText => _suspect.EvidenceText;

        public string DrugTestResultText { get; private set; } = string.Empty;
        public bool IsDrugTestButtonEnabled { get; private set; }

        /// <summary>Raised when <see cref="DrugTestResultText"/> or
        /// <see cref="IsDrugTestButtonEnabled"/> changes.</summary>
        public event Action? StateChanged;

        /// <summary>Raised after a verdict has been recorded. The view typically
        /// hides this panel, shows the status HUD, and refreshes the HUD
        /// counters in response.</summary>
        public event Action? VerdictRecorded;

        /// <summary>Raised when the panel should close for any reason other than
        /// a verdict (e.g. user pressed the close button).</summary>
        public event Action? CloseRequested;

        public SuspectDetailViewModel(SuspectData suspect, ILevelController levels)
        {
            _suspect = suspect ?? throw new ArgumentNullException(nameof(suspect));
            _levels = levels ?? throw new ArgumentNullException(nameof(levels));
            RefreshDrugTestState();
        }

        public void RequestDrugTest()
        {
            if (!IsDrugTestButtonEnabled) return;
            if (!_levels.UseDrugTest()) return;
            _levels.RecordDrugTest(_suspect, _suspect.DrugTestResult);
            RefreshDrugTestState();
        }

        public void SelectVerdict(SuspectRole role)
        {
            _levels.RecordJudgedSuspect(_suspect, role);
            VerdictRecorded?.Invoke();
        }

        public void RequestClose()
        {
            CloseRequested?.Invoke();
        }

        public void Dispose()
        {
            // No long-lived subscriptions on GameServices yet. Placeholder so
            // future changes (e.g. listening to IVerdictRecorder.VerdictRecorded)
            // have a single place to clean up.
        }

        private void RefreshDrugTestState()
        {
            var alreadyTested = _levels.HasDrugTestResult(_suspect);
            var hasTestsRemaining = _levels.DrugTestsRemaining > 0;
            DrugTestResultText = alreadyTested
                ? _levels.GetDrugTestResult(_suspect).ToDisplayName()
                : string.Empty;
            IsDrugTestButtonEnabled = !alreadyTested && hasTestsRemaining;
            StateChanged?.Invoke();
        }
    }
}
