#nullable enable
using System;

namespace CriminalCase2.ViewModels
{
    /// <summary>
    /// ViewModel for the in-level tutorial panel. Pure C# (no Unity types)
    /// so it can be exercised by EditMode tests without instantiating a
    /// <see cref="UnityEngine.MonoBehaviour"/>.
    ///
    /// Pure command router - the tutorial panel has no state of its own to
    /// observe, only two user intents: close (advance past the tutorial) and
    /// replay the intro video. UI orchestration (hide panels, show the HUD,
    /// transition to the Investigation state, open the video player) stays in
    /// the view; the VM signals intent through the two events.
    /// </summary>
    public sealed class TutorialUIViewModel : IDisposable
    {
        private bool _disposed;

        /// <summary>Raised when the user dismisses the tutorial. The view
        /// typically hides all panels, shows the status HUD, and transitions
        /// to the Investigation state in response.</summary>
        public event Action? CloseRequested;

        /// <summary>Raised when the user presses the replay-video button.
        /// The view typically opens the video player in response.</summary>
        public event Action? ReplayVideoRequested;

        public void RequestClose()
        {
            ThrowIfDisposed();
            CloseRequested?.Invoke();
        }

        public void RequestReplayVideo()
        {
            ThrowIfDisposed();
            ReplayVideoRequested?.Invoke();
        }

        public void Dispose()
        {
            if (_disposed) return;
            CloseRequested = null;
            ReplayVideoRequested = null;
            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(TutorialUIViewModel));
        }
    }
}
