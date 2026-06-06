#nullable enable
using UnityEngine;
using UnityEngine.UIElements;
using CriminalCase2.Services;
using CriminalCase2.ViewModels;

namespace CriminalCase2.UI
{
    public class StatusHUD : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;

        private Button _hudButton = null!;
        private StatusHUDViewModel? _viewModel;

        private bool _isBound;

        public void Initialize()
        {
            if (!_isBound) BindUI();
            if (_viewModel == null) CreateViewModel();
            OnViewModelStateChanged();
        }

        public void UpdateButtonText()
        {
            _viewModel?.Refresh();
        }

        private void OnEnable()
        {
            if (_document != null && _document.rootVisualElement != null)
            {
                BindUI();
                if (_viewModel == null) CreateViewModel();
                OnViewModelStateChanged();
            }
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

            _hudButton = root.Q<Button>(UIConstants.StatusHud.Button);
            if (_hudButton != null)
            {
                _hudButton.clicked += OnHudButtonClicked;
            }

            _isBound = true;
        }

        private void UnbindUI()
        {
            if (_hudButton != null)
            {
                _hudButton.clicked -= OnHudButtonClicked;
            }
            _isBound = false;
        }

        private void CreateViewModel()
        {
            var levels = GameServices.Levels;
            if (levels == null) return;

            _viewModel = new StatusHUDViewModel(levels);
            _viewModel.StateChanged += OnViewModelStateChanged;
            _viewModel.OpenCheckStatusRequested += OnOpenCheckStatusRequested;
        }

        private void DisposeViewModel()
        {
            if (_viewModel == null) return;
            _viewModel.StateChanged -= OnViewModelStateChanged;
            _viewModel.OpenCheckStatusRequested -= OnOpenCheckStatusRequested;
            _viewModel.Dispose();
            _viewModel = null;
        }

        private void OnViewModelStateChanged()
        {
            if (_viewModel == null || _hudButton == null) return;
            _hudButton.text = _viewModel.ButtonText;
        }

        private void OnOpenCheckStatusRequested()
        {
            GameServices.UI?.ShowCheckStatus();
        }

        private void OnHudButtonClicked()
        {
            _viewModel?.RequestOpenCheckStatus();
        }
    }
}
