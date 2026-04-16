using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using CriminalCase2.Data;
using CriminalCase2.Managers;
using CriminalCase2.Utils;

namespace CriminalCase2.UI
{
    /// <summary>
    /// Video player controller using UGUI.
    /// Handles video playback with WebGL support.
    /// </summary>
    public class VideoPlayerUI : MonoBehaviour
    {
        [Header("Video")]
        [SerializeField] private VideoPlayer _videoPlayer;

        [Header("UGUI References")]
        [SerializeField] private GameObject _playScreen;
        [SerializeField] private GameObject _videoScreen;
        [SerializeField] private RawImage _videoRawImage;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _skipButton;
        [SerializeField] private Text _titleLabel;
        [SerializeField] private Text _subtitleLabel;

        [Header("Play Screen Text")]
        [SerializeField] private string _titleText = "Criminal Case 2";
        [SerializeField] private string _subtitleText = "Klik di bawah untuk memulai investigasi";
        [SerializeField] private string _playButtonText = "Putar Intro";
        [SerializeField] private string _skipButtonText = "Lewati >>";

        [Header("Timeout")]
        [SerializeField] private float _prepareTimeoutSeconds = 10f;

        private Texture2D _fallbackTexture;
        private bool _isPlaying;
        private bool _isInitialized;

        #region Unity Lifecycle

        private void Awake()
        {
            LoggingUtility.LogDebug("Video", "VideoPlayerUI Awake");

            if (_videoPlayer == null)
                _videoPlayer = GetComponent<VideoPlayer>();

            AutoFindReferences();
        }

        private void OnEnable()
        {
            if (!_isInitialized)
            {
                SetupVideoPlayer();
                SetupUI();
                _isInitialized = true;
            }

            ShowPlayScreen();
        }

        private void OnDisable()
        {
            CleanupVideoPlayer();
            UnbindButtons();
            _isPlaying = false;

            if (_videoRawImage != null)
                _videoRawImage.texture = null;
        }

        private void LateUpdate()
        {
            if (_isPlaying && _videoPlayer != null && _videoRawImage != null)
            {
                _videoRawImage.color = Color.white;
                var tex = _videoPlayer.texture;
                if (tex != null && _videoRawImage.texture != tex)
                    _videoRawImage.texture = tex;
            }
        }

        #endregion

        #region Setup

        private void AutoFindReferences()
        {
            if (_playScreen == null)
            {
                var t = transform.Find("PlayScreen");
                if (t != null) _playScreen = t.gameObject;
            }

            if (_videoScreen == null)
            {
                var t = transform.Find("VideoScreen");
                if (t != null) _videoScreen = t.gameObject;
            }

            if (_videoRawImage == null)
            {
                var t = transform.Find("VideoScreen/VideoRawImage");
                if (t != null) _videoRawImage = t.GetComponent<RawImage>();
            }

            if (_playButton == null)
            {
                var t = transform.Find("PlayScreen/PlayButton");
                if (t != null) _playButton = t.GetComponent<Button>();
            }

            if (_skipButton == null)
            {
                var t = transform.Find("VideoScreen/SkipButtonContainer/SkipButton");
                if (t != null) _skipButton = t.GetComponent<Button>();
            }

            if (_titleLabel == null)
            {
                var t = transform.Find("PlayScreen/TitleLabel");
                if (t != null) _titleLabel = t.GetComponent<Text>();
            }

            if (_subtitleLabel == null)
            {
                var t = transform.Find("PlayScreen/SubtitleLabel");
                if (t != null) _subtitleLabel = t.GetComponent<Text>();
            }
        }

        private void SetupUI()
        {
            if (_titleLabel != null)
                _titleLabel.text = _titleText;
            if (_subtitleLabel != null)
                _subtitleLabel.text = _subtitleText;
            if (_playButton != null)
            {
                var playBtnText = _playButton.GetComponentInChildren<Text>();
                if (playBtnText != null) playBtnText.text = _playButtonText;
            }
            if (_skipButton != null)
            {
                var skipBtnText = _skipButton.GetComponentInChildren<Text>();
                if (skipBtnText != null) skipBtnText.text = _skipButtonText;
            }

            BindButtons();
        }

        private void BindButtons()
        {
            if (_playButton != null)
                _playButton.onClick.AddListener(OnPlayClicked);
            if (_skipButton != null)
                _skipButton.onClick.AddListener(OnSkipClicked);
        }

        private void UnbindButtons()
        {
            if (_playButton != null)
                _playButton.onClick.RemoveListener(OnPlayClicked);
            if (_skipButton != null)
                _skipButton.onClick.RemoveListener(OnSkipClicked);
        }

        private void SetupVideoPlayer()
        {
            if (_videoPlayer == null)
            {
                LoggingUtility.Error("Video", "VideoPlayer is null in SetupVideoPlayer!");
                return;
            }

            _videoPlayer.renderMode = VideoRenderMode.APIOnly;
            _videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
            _videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            _videoPlayer.skipOnDrop = true;
            _videoPlayer.waitForFirstFrame = false;

            _videoPlayer.errorReceived += OnVideoError;
            _videoPlayer.loopPointReached += OnVideoFinished;

            // Fix video orientation - videos render upside-down by default
            if (_videoRawImage != null)
            {
                _videoRawImage.uvRect = new Rect(0, 0, 1, 1);
            }

            LoadVideoSource();
        }

        private void LoadVideoSource()
        {
            if (GameManager.Instance == null)
            {
                LoggingUtility.Warning("Video", "GameManager.Instance is null, cannot load video");
                return;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL: Load from URL
            string fileName = GameManager.Instance.IntroVideoFileName;
            string url = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
            _videoPlayer.source = VideoSource.Url;
            _videoPlayer.url = url;
            LoggingUtility.LogVideo($"WebGL mode: URL={url}");
#else
            // Editor/Standalone: Try VideoClip first
            if (GameManager.Instance.GlobalIntroVideo != null)
            {
                _videoPlayer.source = VideoSource.VideoClip;
                _videoPlayer.clip = GameManager.Instance.GlobalIntroVideo;
                LoggingUtility.LogVideo($"Editor mode: VideoClip={_videoPlayer.clip.name}");
            }
            else
            {
                string fileName = GameManager.Instance.IntroVideoFileName;
                string url = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
                _videoPlayer.source = VideoSource.Url;
                _videoPlayer.url = url;
                LoggingUtility.LogVideo($"Editor mode (no clip): URL={url}");
            }
#endif
        }

        private void CleanupVideoPlayer()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.errorReceived -= OnVideoError;
                _videoPlayer.loopPointReached -= OnVideoFinished;
                _videoPlayer.Stop();
            }
        }

        #endregion

        #region UI Event Handlers

        private void OnPlayClicked()
        {
            LoggingUtility.LogVideo("Play button clicked");

            if (_videoPlayer == null)
            {
                LoggingUtility.Warning("Video", "No VideoPlayer, skipping to investigation");
                OnVideoFinishedOrSkipped();
                return;
            }

            bool hasSource = _videoPlayer.source == VideoSource.VideoClip
                ? _videoPlayer.clip != null
                : !string.IsNullOrEmpty(_videoPlayer.url);

            if (!hasSource)
            {
                LoggingUtility.Warning("Video", "No video source assigned, skipping to investigation");
                OnVideoFinishedOrSkipped();
                return;
            }

            ShowVideoScreen();

            if (_fallbackTexture == null)
            {
                _fallbackTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
                var pixels = new Color32[4];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(0, 0, 0, 255);
                _fallbackTexture.SetPixels32(pixels);
                _fallbackTexture.Apply();
            }

            if (_videoRawImage != null && _videoPlayer.texture == null)
            {
                _videoRawImage.color = Color.white;
                _videoRawImage.texture = _fallbackTexture;
            }

            StartCoroutine(PrepareAndPlay());
        }

        private System.Collections.IEnumerator PrepareAndPlay()
        {
            LoggingUtility.LogVideo("PrepareAndPlay: starting Prepare()");
            _videoPlayer.Prepare();

            if (_videoPlayer.isPrepared)
            {
                LoggingUtility.LogVideo("Video already prepared, playing immediately");
            }
            else
            {
                float elapsed = 0f;
                while (!_videoPlayer.isPrepared)
                {
                    if (_videoPlayer == null)
                    {
                        LoggingUtility.Warning("Video", "VideoPlayer destroyed during prepare");
                        yield break;
                    }

                    elapsed += Time.unscaledDeltaTime;
                    if (elapsed > _prepareTimeoutSeconds)
                    {
                        LoggingUtility.Warning($"Video", $"Prepare timeout after {_prepareTimeoutSeconds}s");
                        break;
                    }

                    yield return null;
                }
            }

            if (_videoRawImage != null && _videoPlayer.texture != null)
            {
                _videoRawImage.color = Color.white;
                _videoRawImage.texture = _videoPlayer.texture;
            }

            _isPlaying = true;
            _videoPlayer.Play();
            LoggingUtility.LogVideo("Play() called");
        }

        private void OnSkipClicked()
        {
            LoggingUtility.LogVideo("Skip button clicked");
            StopVideo();
        }

        #endregion

        #region Video Event Handlers

        private void OnVideoFinished(VideoPlayer vp)
        {
            LoggingUtility.LogVideo("Video finished playing");
            _isPlaying = false;
            OnVideoFinishedOrSkipped();
        }

        private void OnVideoError(VideoPlayer vp, string message)
        {
            LoggingUtility.Error("Video", $"Video error: {message}");
            _isPlaying = false;
            OnVideoFinishedOrSkipped();
        }

        private void StopVideo()
        {
            StopAllCoroutines();
            _isPlaying = false;

            if (_videoPlayer != null)
                _videoPlayer.Stop();

            if (_videoRawImage != null)
                _videoRawImage.texture = null;

            OnVideoFinishedOrSkipped();
        }

        private void OnVideoFinishedOrSkipped()
        {
            ShowPlayScreen();
            UIManager.Instance?.HideAllPanels();
            GameManager.Instance?.SetState(GameState.ClueSearch);
        }

        #endregion

        #region Screen Management

        public void ShowPlayScreen()
        {
            if (_playScreen != null)
                _playScreen.SetActive(true);
            if (_videoScreen != null)
                _videoScreen.SetActive(false);
        }

        private void ShowVideoScreen()
        {
            if (_playScreen != null)
                _playScreen.SetActive(false);
            if (_videoScreen != null)
                _videoScreen.SetActive(true);
        }

        #endregion

        private void OnDestroy()
        {
            if (_fallbackTexture != null)
            {
                Destroy(_fallbackTexture);
                _fallbackTexture = null;
            }
        }
    }
}
