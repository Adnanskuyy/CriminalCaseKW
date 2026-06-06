#nullable enable
using UnityEngine;
using UnityEngine.UIElements;
using CriminalCase2.Data;
using CriminalCase2.Services;
using CriminalCase2.ViewModels;
using System.Collections.Generic;

namespace CriminalCase2.UI
{
    public class CheckStatusUI : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;

        private CheckStatusViewModel? _viewModel;

        private VisualElement _container = null!;
        private VisualElement _emptyState = null!;
        private Button _closeButton = null!;
        private Button _checkResultButton = null!;

        private bool _isBound;

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

        private void BindUI()
        {
            if (_document == null || _isBound) return;

            var root = _document.rootVisualElement;
            if (root == null) return;

            _container = root.Q<VisualElement>(UIConstants.CheckStatus.Container);
            _emptyState = root.Q<VisualElement>(UIConstants.CheckStatus.Empty);
            _closeButton = root.Q<Button>(UIConstants.CheckStatus.CloseButton);
            _checkResultButton = root.Q<Button>(UIConstants.CheckStatus.CheckResultButton);

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
            _emptyState = null;
            _isBound = false;
        }

        private void CreateViewModel()
        {
            var levels = GameServices.Levels;
            if (levels == null) return;
            _viewModel = new CheckStatusViewModel(levels);
            _viewModel.StateChanged += OnViewModelStateChanged;
            _viewModel.CloseRequested += OnViewModelCloseRequested;
            _viewModel.SubmitRequested += OnViewModelSubmitRequested;
        }

        private void DisposeViewModel()
        {
            if (_viewModel == null) return;
            _viewModel.StateChanged -= OnViewModelStateChanged;
            _viewModel.CloseRequested -= OnViewModelCloseRequested;
            _viewModel.SubmitRequested -= OnViewModelSubmitRequested;
            _viewModel.Dispose();
            _viewModel = null;
        }

        public void Populate(IReadOnlyList<VerdictRecord> records)
        {
            if (!_isBound) BindUI();
            if (_container == null) return;
            if (_viewModel == null) CreateViewModel();
            if (_viewModel == null) return;

            _viewModel.SetRecords(records);
            Refresh();
        }

        private void Refresh()
        {
            if (_viewModel == null || _container == null) return;

            _container.Clear();

            if (_viewModel.IsEmpty)
            {
                if (_container != null) _container.style.display = DisplayStyle.None;
                if (_emptyState != null) _emptyState.style.display = DisplayStyle.Flex;
            }
            else
            {
                if (_container != null) _container.style.display = DisplayStyle.Flex;
                if (_emptyState != null) _emptyState.style.display = DisplayStyle.None;

                foreach (var entry in _viewModel.Entries)
                {
                    _container.Add(CreateStatusEntry(entry));
                }
            }

            if (_checkResultButton != null)
            {
                _checkResultButton.SetEnabled(_viewModel.CanSubmit);
                _checkResultButton.text = _viewModel.SubmitButtonText;
            }
        }

        private VisualElement CreateStatusEntry(CheckStatusViewModel.StatusEntry entry)
        {
            var row = new VisualElement();
            row.AddToClassList("check-status-entry");

            var nameLabel = new Label(entry.SuspectName);
            nameLabel.AddToClassList("check-status-name");

            var verdictLabel = new Label($"Vonis Anda: {entry.PlayerVerdictDisplay}");
            verdictLabel.AddToClassList("check-status-verdict");

            row.Add(nameLabel);
            row.Add(verdictLabel);

            return row;
        }

        private void OnViewModelStateChanged()
        {
            Refresh();
        }

        private void OnViewModelCloseRequested()
        {
            GameServices.UI?.HideCheckStatus();
        }

        private void OnViewModelSubmitRequested()
        {
            GameServices.GameState?.SetState(GameState.Results);
        }

        private void OnCloseClicked()
        {
            _viewModel?.RequestClose();
        }

        private void OnCheckResultClicked()
        {
            _viewModel?.RequestSubmit();
        }
    }
}
