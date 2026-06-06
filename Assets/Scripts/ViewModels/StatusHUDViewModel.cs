#nullable enable
using System;
using CriminalCase2.Data;
using CriminalCase2.Domain;

namespace CriminalCase2.ViewModels
{
    /// <summary>
    /// ViewModel for the persistent in-level HUD button. Pure C# (no Unity
    /// types beyond <see cref="ILevelController"/>) so it can be exercised by
    /// EditMode tests without instantiating a <see cref="UnityEngine.MonoBehaviour"/>.
    ///
    /// Owns the formatted button text (driven by the current level's suspect
    /// count and how many have been judged) and signals user intent through
    /// the <see cref="OpenCheckStatusRequested"/> event. UI orchestration
    /// (which panel to show on click) stays in the view.
    /// </summary>
    public sealed class StatusHUDViewModel : IDisposable
    {
        private readonly ILevelController _levels;
        private bool _disposed;

        /// <summary>Raised when <see cref="ButtonText"/> changes.</summary>
        public event Action? StateChanged;

        /// <summary>Raised when the user presses the HUD button. The view
        /// typically opens the check-status panel in response.</summary>
        public event Action? OpenCheckStatusRequested;

        public string ButtonText { get; private set; } = string.Empty;

        public StatusHUDViewModel(ILevelController levels)
        {
            _levels = levels ?? throw new ArgumentNullException(nameof(levels));
            _levels.LevelLoaded += OnLevelLoaded;
            RecomputeButtonText();
        }

        /// <summary>Re-reads the level state and recomputes <see cref="ButtonText"/>.
        /// Idempotent: only raises <see cref="StateChanged"/> when the text
        /// actually changes. Safe to call from the view's OnEnable.</summary>
        public void Refresh()
        {
            ThrowIfDisposed();
            RecomputeButtonText();
        }

        public void RequestOpenCheckStatus()
        {
            ThrowIfDisposed();
            OpenCheckStatusRequested?.Invoke();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _levels.LevelLoaded -= OnLevelLoaded;
            _disposed = true;
        }

        private void OnLevelLoaded(LevelConfig _)
        {
            RecomputeButtonText();
        }

        private void RecomputeButtonText()
        {
            var judged = _levels.JudgedCount;
            var total = _levels.TotalSuspects;

            string newText;
            if (total <= 0)
            {
                newText = string.Empty;
            }
            else if (judged >= total)
            {
                newText = $"Lihat Hasil ({judged}/{total})";
            }
            else if (judged > 0)
            {
                newText = $"Cek Status ({judged}/{total})";
            }
            else
            {
                newText = $"Cek Status (0/{total})";
            }

            if (!string.Equals(newText, ButtonText, StringComparison.Ordinal))
            {
                ButtonText = newText;
                StateChanged?.Invoke();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(StatusHUDViewModel));
        }
    }
}
