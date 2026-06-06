#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using CriminalCase2.Data;
using CriminalCase2.Domain;

namespace CriminalCase2.ViewModels
{
    public sealed class CheckStatusViewModel : IDisposable
    {
        private readonly ILevelController _levels;
        private bool _disposed;
        private IReadOnlyList<VerdictRecord> _records = Array.Empty<VerdictRecord>();

        public event Action? StateChanged;
        public event Action? CloseRequested;
        public event Action? SubmitRequested;

        public IReadOnlyList<StatusEntry> Entries { get; private set; } = Array.Empty<StatusEntry>();
        public bool IsEmpty => _records.Count == 0;
        public bool CanSubmit => _levels.AllSuspectsJudged;
        public string SubmitButtonText
        {
            get
            {
                if (CanSubmit) return "Kirim Vonis Akhir";
                int remaining = _levels.TotalSuspects - _levels.JudgedCount;
                return $"Kirim Vonis Akhir ({remaining} tersisa)";
            }
        }

        public CheckStatusViewModel(ILevelController levels)
        {
            _levels = levels ?? throw new ArgumentNullException(nameof(levels));
            _levels.LevelLoaded += OnLevelLoaded;
        }

        public void SetRecords(IReadOnlyList<VerdictRecord> records)
        {
            ThrowIfDisposed();
            if (records == null) throw new ArgumentNullException(nameof(records));
            _records = records;
            Entries = records
                .Select(r => new StatusEntry(r.Suspect.SuspectName, r.PlayerChoice.ToDisplayName()))
                .ToList();
            StateChanged?.Invoke();
        }

        private void OnLevelLoaded(LevelConfig _) => StateChanged?.Invoke();

        public void RequestClose()
        {
            ThrowIfDisposed();
            CloseRequested?.Invoke();
        }

        public void RequestSubmit()
        {
            ThrowIfDisposed();
            if (!CanSubmit) return;
            SubmitRequested?.Invoke();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _levels.LevelLoaded -= OnLevelLoaded;
            _disposed = true;
            StateChanged = null;
            CloseRequested = null;
            SubmitRequested = null;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(CheckStatusViewModel));
        }

        public sealed class StatusEntry
        {
            public string SuspectName { get; }
            public string PlayerVerdictDisplay { get; }

            public StatusEntry(string suspectName, string playerVerdictDisplay)
            {
                SuspectName = suspectName;
                PlayerVerdictDisplay = playerVerdictDisplay;
            }
        }
    }
}
