using UnityEngine;
using UnityEngine.UIElements;
using CriminalCase2.Data;
using CriminalCase2.Services;

namespace CriminalCase2.UI
{
    public class TutorialUI : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;

        private Button _closeButton;
        private Button _replayVideoButton;

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

            _closeButton = _document.rootVisualElement.Q<Button>(UIConstants.Tutorial.CloseButton);
            if (_closeButton != null)
            {
                _closeButton.clicked += OnCloseClicked;
            }

            _replayVideoButton = _document.rootVisualElement.Q<Button>(UIConstants.Tutorial.ReplayVideoButton);
            if (_replayVideoButton != null)
            {
                _replayVideoButton.clicked += OnReplayVideoClicked;
            }
        }

        private void UnbindUI()
        {
            if (_closeButton != null)
            {
                _closeButton.clicked -= OnCloseClicked;
                _closeButton = null;
            }

            if (_replayVideoButton != null)
            {
                _replayVideoButton.clicked -= OnReplayVideoClicked;
                _replayVideoButton = null;
            }
        }

        private void OnCloseClicked()
        {
            GameServices.UI?.HideAllPanels();
            GameServices.UI?.ShowStatusHUD();
            GameServices.GameState?.SetState(GameState.Investigation);
        }

        private void OnReplayVideoClicked()
        {
            GameServices.UI?.ShowVideoPlayer();
        }
    }
}
