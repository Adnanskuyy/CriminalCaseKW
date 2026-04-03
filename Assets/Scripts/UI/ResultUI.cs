using UnityEngine;
using UnityEngine.UIElements;
using CriminalCase2.Data;
using CriminalCase2.Managers;
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

            for (int i = 0; i < records.Count; i++)
            {
                var record = records[i];
                var entry = new VisualElement();
                entry.AddToClassList("result-entry");

                var nameLabel = new Label($"{i + 1}. {record.Suspect.SuspectName}");
                var choiceLabel = new Label($"Your verdict: {record.PlayerChoice}");
                var correctLabel = new Label($"Correct: {record.CorrectAnswer}");
                var feedbackLabel = new Label(record.FeedbackText);

                entry.Add(nameLabel);
                entry.Add(choiceLabel);
                entry.Add(correctLabel);
                entry.Add(feedbackLabel);

                _resultsContainer.Add(entry);
            }
        }

        private void OnNextLevelClicked()
        {
            GameManager.Instance?.AdvanceToNextLevel();
            UIManager.Instance?.HideAllPanels();
        }
    }
}
