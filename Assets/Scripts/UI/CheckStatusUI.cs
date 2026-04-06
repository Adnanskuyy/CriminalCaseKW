using UnityEngine;
using UnityEngine.UIElements;
using CriminalCase2.Data;
using CriminalCase2.Managers;
using System.Collections.Generic;

namespace CriminalCase2.UI
{
    public class CheckStatusUI : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;

        private VisualElement _container;
        private Button _closeButton;
        private Button _checkResultButton;
        private bool _isBound;

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
            if (_document == null || _isBound) return;

            var root = _document.rootVisualElement;
            if (root == null) return;

            _container = root.Q<VisualElement>("check-status-container");
            _closeButton = root.Q<Button>("check-status-close-button");
            _checkResultButton = root.Q<Button>("check-result-button");

            if (_closeButton != null)
            {
                _closeButton.clicked += OnCloseClicked;
            }

            if (_checkResultButton != null)
            {
                _checkResultButton.clicked += OnCheckResultClicked;
            }

            _isBound = true;
        }

        private void UnbindUI()
        {
            if (_closeButton != null)
            {
                _closeButton.clicked -= OnCloseClicked;
                _closeButton = null;
            }

            if (_checkResultButton != null)
            {
                _checkResultButton.clicked -= OnCheckResultClicked;
                _checkResultButton = null;
            }

            _container = null;
            _isBound = false;
        }

        public void Populate(IReadOnlyList<VerdictRecord> records)
        {
            if (!_isBound) BindUI();
            if (_container == null) return;

            _container.Clear();

            foreach (var record in records)
            {
                var entry = CreateStatusEntry(record);
                _container.Add(entry);
            }

            // Update check result button state
            if (_checkResultButton != null && LevelManager.Instance != null)
            {
                bool allJudged = LevelManager.Instance.AllSuspectsJudged;
                _checkResultButton.SetEnabled(allJudged);
                
                if (!allJudged)
                {
                    int remaining = LevelManager.Instance.TotalSuspects - LevelManager.Instance.JudgedCount;
                    _checkResultButton.text = $"Check to see result ({remaining} remaining)";
                }
                else
                {
                    _checkResultButton.text = "Check to see result";
                }
            }
        }

        private VisualElement CreateStatusEntry(VerdictRecord record)
        {
            var entry = new VisualElement();
            entry.AddToClassList("check-status-entry");

            var nameLabel = new Label(record.Suspect.SuspectName);
            nameLabel.AddToClassList("check-status-name");

            var verdictLabel = new Label($"Your verdict: {record.PlayerChoice}");
            verdictLabel.AddToClassList("check-status-verdict");

            entry.Add(nameLabel);
            entry.Add(verdictLabel);

            return entry;
        }

        private void OnCloseClicked()
        {
            UIManager.Instance?.HideCheckStatus();
        }

        private void OnCheckResultClicked()
        {
            if (LevelManager.Instance != null && LevelManager.Instance.AllSuspectsJudged)
            {
                GameManager.Instance?.SetState(GameState.Results);
            }
        }
    }
}
