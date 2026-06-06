#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using CriminalCase2.Data;
using CriminalCase2.Domain;

namespace CriminalCase2.ViewModels
{
    public sealed class ResultViewModel : IDisposable
    {
        private bool _disposed;

        public event Action? StateChanged;
        public event Action? NextLevelRequested;

        public IReadOnlyList<ResultEntry> Entries { get; private set; } = Array.Empty<ResultEntry>();

        public ResultViewModel() { }

        public void SetRecords(IReadOnlyList<VerdictRecord> records)
        {
            ThrowIfDisposed();
            if (records == null) throw new ArgumentNullException(nameof(records));
            Entries = records
                .Select((r, i) => new ResultEntry(
                    i + 1,
                    r.Suspect.SuspectName,
                    r.PlayerChoice.ToDisplayName(),
                    r.CorrectAnswer.ToDisplayName(),
                    r.FeedbackText))
                .ToList();
            StateChanged?.Invoke();
        }

        public void RequestNextLevel()
        {
            ThrowIfDisposed();
            NextLevelRequested?.Invoke();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            StateChanged = null;
            NextLevelRequested = null;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ResultViewModel));
        }

        public sealed class ResultEntry
        {
            public int Index { get; }
            public string SuspectName { get; }
            public string PlayerChoiceDisplay { get; }
            public string CorrectAnswerDisplay { get; }
            public string FeedbackText { get; }

            public ResultEntry(
                int index,
                string suspectName,
                string playerChoiceDisplay,
                string correctAnswerDisplay,
                string feedbackText)
            {
                Index = index;
                SuspectName = suspectName;
                PlayerChoiceDisplay = playerChoiceDisplay;
                CorrectAnswerDisplay = correctAnswerDisplay;
                FeedbackText = feedbackText;
            }
        }
    }
}
