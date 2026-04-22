using UnityEngine;
using UnityEngine.UIElements;
using CriminalCase2.Data;
using CriminalCase2.Managers;
using CriminalCase2.Services;
using CriminalCase2.Services.Interfaces;
using CriminalCase2.Utils;
using System.Collections.Generic;

namespace CriminalCase2.UI
{
    public class ResultUI : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;

        private Button _nextLevelButton;
        private VisualElement _resultsContainer;

        public void Populate(IReadOnlyList<VerdictRecord> records)
        {
            BuildResultsList(records);
        }

        private void OnEnable()
        {
            BindUI();
        }

        private void OnDisable()
        {
            UnbindUI();
        }

        private void BindUI()
        {
            if (_document == null) return;

            var root = _document.rootVisualElement;

            _resultsContainer = root.Q<VisualElement>("results-container");
            _nextLevelButton = root.Q<Button>("next-level-button");
            if (_nextLevelButton != null)
            {
                _nextLevelButton.clicked += OnNextLevelClicked;
            }
        }

        private void UnbindUI()
        {
            if (_nextLevelButton != null)
            {
                _nextLevelButton.clicked -= OnNextLevelClicked;
                _nextLevelButton = null;
            }
        }

        private void BuildResultsList(IReadOnlyList<VerdictRecord> records)
        {
            if (_resultsContainer == null) return;

            _resultsContainer.Clear();

            var matchingService = ServiceLocator.Get<IClueMatchingService>();
            int correctMatches = matchingService?.CorrectMatchCount ?? 0;
            int totalClues = matchingService?.TotalClueCount ?? 0;
            float matchingAccuracy = matchingService?.MatchingAccuracy ?? 0f;

            var scoreHeader = new VisualElement();
            scoreHeader.AddToClassList("result-score-header");

            var matchingLabel = new Label($"Kecocokan Petunjuk: {correctMatches}/{totalClues} ({matchingAccuracy:P0})");
            matchingLabel.AddToClassList("result-matching-score");

            int correctVerdicts = 0;
            foreach (var r in records)
                if (r.IsCorrect) correctVerdicts++;

            var verdictLabel = new Label($"Akurasi Vonis: {correctVerdicts}/{records.Count} ({(records.Count > 0 ? (float)correctVerdicts / records.Count : 0f):P0})");
            verdictLabel.AddToClassList("result-verdict-score");

            float overall = records.Count > 0 ? (matchingAccuracy + (float)correctVerdicts / records.Count) / 2f : 0f;
            var overallLabel = new Label($"Skor Keseluruhan: {overall:P0}");
            overallLabel.AddToClassList("result-overall-score");

            scoreHeader.Add(matchingLabel);
            scoreHeader.Add(verdictLabel);
            scoreHeader.Add(overallLabel);
            _resultsContainer.Add(scoreHeader);

            for (int i = 0; i < records.Count; i++)
            {
                var record = records[i];
                var entry = new VisualElement();
                entry.AddToClassList("result-entry");

                var nameLabel = new Label($"{i + 1}. {record.Suspect.SuspectName}");

                var matchedCluesText = GetMatchedCluesText(record.Suspect, matchingService);
                var cluesLabel = new Label($"Petunjuk Terpasang: {matchedCluesText}");

                var choiceLabel = new Label($"Vonis Anda: {record.PlayerChoice.ToDisplayName()}");
                var correctLabel = new Label($"Jawaban Benar: {record.CorrectAnswer.ToDisplayName()}");
                var feedbackLabel = new Label(record.FeedbackText);

                nameLabel.AddToClassList("result-suspect-name");
                cluesLabel.AddToClassList("result-clue-matches");

                entry.Add(nameLabel);
                entry.Add(cluesLabel);
                entry.Add(choiceLabel);
                entry.Add(correctLabel);
                entry.Add(feedbackLabel);

                _resultsContainer.Add(entry);
            }
        }

        private string GetMatchedCluesText(SuspectData suspect, IClueMatchingService matchingService)
        {
            if (matchingService == null || suspect == null) return "-";

            var clues = matchingService.GetCluesForSuspect(suspect);
            if (clues == null || clues.Count == 0) return "Tidak ada";

            var names = new List<string>();
            foreach (var clue in clues)
            {
                bool correct = clue.LinkedSuspect != null && clue.LinkedSuspect == suspect;
                names.Add($"{clue.ClueName}{(correct ? " ✓" : " ✗")}");
            }
            return string.Join(", ", names);
        }

        private void OnNextLevelClicked()
        {
            GameManager.Instance?.AdvanceToNextLevel();
            UIManager.Instance?.HideAllPanels();
        }
    }
}
