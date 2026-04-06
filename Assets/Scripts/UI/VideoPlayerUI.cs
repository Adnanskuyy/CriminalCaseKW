using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
using CriminalCase2.Managers;
using CriminalCase2.Data;

namespace CriminalCase2.UI
{
    public class VideoPlayerUI : MonoBehaviour
    {
        [SerializeField] private VideoPlayer _videoPlayer;
        [SerializeField] private UIDocument _document;
        [SerializeField] private Camera _targetCamera;

        private Button _playButton;
        private Button _skipButton;
        private VisualElement _videoContainer;
        private VisualElement _playContainer;

        private void Awake()
        {
            if (_videoPlayer == null)
                _videoPlayer = GetComponent<VideoPlayer>();
            if (_document == null)
                _document = GetComponent<UIDocument>();
            if (_targetCamera == null)
                _targetCamera = Camera.main;
        }

        private void OnEnable()
        {
            BindUI();
            SetupVideoPlayer();
            ShowPlayScreen();
        }

        private void OnDisable()
        {
            UnbindUI();
            if (_videoPlayer != null)
            {
                _videoPlayer.loopPointReached -= OnVideoFinished;
            }
        }

        private void BindUI()
        {
            if (_document == null || _document.rootVisualElement == null) return;

            _playButton = _document.rootVisualElement.Q<Button>("play-button");
            _skipButton = _document.rootVisualElement.Q<Button>("skip-button");
            _videoContainer = _document.rootVisualElement.Q<VisualElement>("video-container");
            _playContainer = _document.rootVisualElement.Q<VisualElement>("play-container");

            if (_playButton != null)
            {
                _playButton.clicked += OnPlayClicked;
            }

            if (_skipButton != null)
            {
                _skipButton.clicked += OnSkipClicked;
                _skipButton.style.display = DisplayStyle.None;
            }
        }

        private void UnbindUI()
        {
            if (_playButton != null)
            {
                _playButton.clicked -= OnPlayClicked;
                _playButton = null;
            }

            if (_skipButton != null)
            {
                _skipButton.clicked -= OnSkipClicked;
                _skipButton = null;
            }

            _videoContainer = null;
            _playContainer = null;
        }

        private void SetupVideoPlayer()
        {
            if (_videoPlayer == null || _targetCamera == null) return;

            _videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
            _videoPlayer.targetCamera = _targetCamera;
            _videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
            _videoPlayer.loopPointReached += OnVideoFinished;

            if (GameManager.Instance != null && GameManager.Instance.GlobalIntroVideo != null)
            {
                _videoPlayer.clip = GameManager.Instance.GlobalIntroVideo;
            }
        }

        private void ShowPlayScreen()
        {
            if (_playContainer != null)
                _playContainer.style.display = DisplayStyle.Flex;
            if (_videoContainer != null)
                _videoContainer.style.display = DisplayStyle.None;
            if (_skipButton != null)
                _skipButton.style.display = DisplayStyle.None;
        }

        private void ShowVideoScreen()
        {
            if (_playContainer != null)
                _playContainer.style.display = DisplayStyle.None;
            if (_videoContainer != null)
                _videoContainer.style.display = DisplayStyle.Flex;
            if (_skipButton != null)
                _skipButton.style.display = DisplayStyle.Flex;
        }

        private void OnPlayClicked()
        {
            Debug.Log("[VideoPlayerUI] Play button clicked - starting video");
            
            if (_videoPlayer != null && _videoPlayer.clip != null)
            {
                ShowVideoScreen();
                _videoPlayer.Play();
            }
            else
            {
                Debug.LogWarning("[VideoPlayerUI] No video clip assigned, skipping to investigation");
                OnVideoFinishedOrSkipped();
            }
        }

        private void OnSkipClicked()
        {
            Debug.Log("[VideoPlayerUI] Skip button clicked");
            StopVideo();
        }

        private void OnVideoFinished(VideoPlayer vp)
        {
            Debug.Log("[VideoPlayerUI] Video finished playing");
            OnVideoFinishedOrSkipped();
        }

        private void StopVideo()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.Stop();
            }
            OnVideoFinishedOrSkipped();
        }

        private void OnVideoFinishedOrSkipped()
        {
            UIManager.Instance?.HideAllPanels();
            GameManager.Instance?.SetState(GameState.Investigation);
        }
    }
}
