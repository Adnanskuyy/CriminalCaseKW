#nullable enable
using UnityEngine;
using UnityEngine.UIElements;
using CriminalCase2.Data;
using CriminalCase2.Services;
using CriminalCase2.ViewModels;
using System.Collections.Generic;

namespace CriminalCase2.UI
{
    public class ResultUI : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;

        private ResultViewModel? _viewModel;

        private Button _nextLevelButton = null!;
        private VisualElement _resultsContainer = null!;

        private bool _isBound;

        public void Populate(IReadOnlyList<VerdictRecord> records)
        {
            if (!_isBound) BindUI();
            if (_resultsContainer == null) return;
            if (_viewModel == null) CreateViewModel();
            if (_viewModel == null) return;

            _viewModel.SetRecords(records);
            Refresh();
        }

        private void OnEnable()
        {
            BindUI();
            CreateViewModel();
        }

        private void OnDisable()
        {
            UnbindUI();
            DisposeViewModel();
        }

        private void CreateViewModel()
        {
            _viewModel = new ResultViewModel();
            _viewModel.StateChanged += OnViewModelStateChanged;
            _viewModel.NextLevelRequested += OnViewModelNextLevelRequested;
        }

        private void DisposeViewModel()
        {
            if (_viewModel == null) return;
            _viewModel.StateChanged -= OnViewModelStateChanged;
            _viewModel.NextLevelRequested -= OnViewModelNextLevelRequested;
            _viewModel.Dispose();
            _viewModel = null;
        }

        private void BindUI()
        {
            if (_document == null || _isBound) return;

            var root = _document.rootVisualElement;
            if (root == null) return;

            _resultsContainer = root.Q<VisualElement>(UIConstants.Result.ResultsContainer);
            _nextLevelButton = root.Q<Button>(UIConstants.Result.NextLevelButton);
            if (_nextLevelButton != null)
            {
                _nextLevelButton.clicked += OnNextLevelClicked;
            }

            _isBound = true;
        }

        private void UnbindUI()
        {
            if (_nextLevelButton != null)
            {
                _nextLevelButton.clicked -= OnNextLevelClicked;
                _nextLevelButton = null;
            }
            _resultsContainer = null;
            _isBound = false;
        }

        private void Refresh()
        {
            if (_viewModel == null || _resultsContainer == null) return;

            _resultsContainer.Clear();

            foreach (var entry in _viewModel.Entries)
            {
                _resultsContainer.Add(CreateResultEntry(entry));
            }
        }

        private VisualElement CreateResultEntry(ResultViewModel.ResultEntry entry)
        {
            var row = new VisualElement();
            row.AddToClassList("result-entry");

            var nameLabel = new Label($"{entry.Index}. {entry.SuspectName}");
            var choiceLabel = new Label($"Vonis Anda: {entry.PlayerChoiceDisplay}");
            var correctLabel = new Label($"Jawaban Benar: {entry.CorrectAnswerDisplay}");
            var feedbackLabel = new Label(entry.FeedbackText);

            row.Add(nameLabel);
            row.Add(choiceLabel);
            row.Add(correctLabel);
            row.Add(feedbackLabel);

            return row;
        }

        private void OnViewModelStateChanged()
        {
            Refresh();
        }

        private void OnViewModelNextLevelRequested()
        {
            GameServices.GameState?.AdvanceToNextLevel();
            GameServices.UI?.HideAllPanels();
        }

        private void OnNextLevelClicked()
        {
            _viewModel?.RequestNextLevel();
        }
    }
}
