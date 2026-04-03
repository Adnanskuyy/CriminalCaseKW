using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace CriminalCase2.UI
{
    public class VideoPlayerUI : MonoBehaviour
    {
        [SerializeField] private VideoPlayer _videoPlayer;
        [SerializeField] private UIDocument _document;

        private Button _skipButton;

        private void OnEnable()
        {
            BindUI();
            PlayVideo();
        }

        private void OnDisable()
        {
            UnbindUI();
        }

        private void BindUI()
        {
            if (_document == null) return;

            _skipButton = _document.rootVisualElement.Q<Button>("skip-button");
            if (_skipButton != null)
            {
                _skipButton.clicked += OnSkipClicked;
            }
        }

        private void UnbindUI()
        {
            if (_skipButton != null)
            {
                _skipButton.clicked -= OnSkipClicked;
                _skipButton = null;
            }
        }

        private void PlayVideo()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.Play();
            }
        }

        private void OnSkipClicked()
        {
            Debug.Log("[VideoPlayerUI] Video skipped.");
            StopVideo();
        }

        private void StopVideo()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.Stop();
            }

            UIManager.Instance?.ShowTutorial();
        }
    }
}
