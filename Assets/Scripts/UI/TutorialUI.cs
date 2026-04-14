using UnityEngine;
using UnityEngine.UIElements;
using CriminalCase2.Data;
using CriminalCase2.Managers;

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

            _closeButton = _document.rootVisualElement.Q<Button>("tutorial-close-button");
            if (_closeButton != null)
            {
                _closeButton.clicked += OnCloseClicked;
            }

            _replayVideoButton = _document.rootVisualElement.Q<Button>("tutorial-replay-video-button");
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
            UIManager.Instance?.HideAllPanels();
            UIManager.Instance?.ShowStatusHUD();
            GameManager.Instance?.SetState(GameState.ClueSearch);
        }

        private void OnReplayVideoClicked()
        {
            UIManager.Instance?.ShowVideoPlayer();
        }
    }
}
