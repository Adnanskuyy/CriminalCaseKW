#nullable enable
using UnityEngine;
using UnityEngine.UIElements;
using CriminalCase2.Data;
using CriminalCase2.Services;
using CriminalCase2.ViewModels;

namespace CriminalCase2.UI
{
    public class TutorialUI : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;

        private Button _closeButton = null!;
        private Button _replayVideoButton = null!;
        private TutorialUIViewModel? _viewModel;

        private void OnEnable()
        {
            BindUI();
        }

        private void OnDisable()
        {
            UnbindUI();
            DisposeViewModel();
        }

        private void BindUI()
        {
            if (_document == null) return;
            var root = _document.rootVisualElement;
            if (root == null) return;

            _closeButton = root.Q<Button>(UIConstants.Tutorial.CloseButton);
            if (_closeButton != null)
            {
                _closeButton.clicked += OnCloseClicked;
            }

            _replayVideoButton = root.Q<Button>(UIConstants.Tutorial.ReplayVideoButton);
            if (_replayVideoButton != null)
            {
                _replayVideoButton.clicked += OnReplayVideoClicked;
            }

            CreateViewModel();
        }

        private void UnbindUI()
        {
            if (_closeButton != null)
            {
                _closeButton.clicked -= OnCloseClicked;
            }

            if (_replayVideoButton != null)
            {
                _replayVideoButton.clicked -= OnReplayVideoClicked;
            }
        }

        private void CreateViewModel()
        {
            _viewModel = new TutorialUIViewModel();
            _viewModel.CloseRequested += OnViewModelCloseRequested;
            _viewModel.ReplayVideoRequested += OnViewModelReplayVideoRequested;
        }

        private void DisposeViewModel()
        {
            if (_viewModel == null) return;
            _viewModel.CloseRequested -= OnViewModelCloseRequested;
            _viewModel.ReplayVideoRequested -= OnViewModelReplayVideoRequested;
            _viewModel.Dispose();
            _viewModel = null;
        }

        private void OnViewModelCloseRequested()
        {
            GameServices.UI?.HideAllPanels();
            GameServices.UI?.ShowStatusHUD();
            GameServices.GameState?.SetState(GameState.Investigation);
        }

        private void OnViewModelReplayVideoRequested()
        {
            GameServices.UI?.ShowVideoPlayer();
        }

        private void OnCloseClicked()
        {
            _viewModel?.RequestClose();
        }

        private void OnReplayVideoClicked()
        {
            _viewModel?.RequestReplayVideo();
        }
    }
}
