using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
using CriminalCase2.Data;
using CriminalCase2.Services;
using CriminalCase2.Utils;
using System.Collections;

namespace CriminalCase2.UI
{
    public class VideoPlayerUI : MonoBehaviour
    {
        [Header("Video")]
        [SerializeField] private VideoPlayer _videoPlayer;

        [Header("UI Toolkit")]
        [SerializeField] private UIDocument _document;
        [SerializeField] private VisualTreeAsset _visualTreeAsset;
        [SerializeField] private StyleSheet _styleSheet;

        [Header("Render Texture")]
        [SerializeField] private Vector2Int _renderTextureSize = new Vector2Int(1920, 1080);

        [Header("Play Screen Text")]
        [SerializeField] private string _titleText = "Criminal Case 2";
        [SerializeField] private string _subtitleText = "Klik di bawah untuk memulai investigasi";
        [SerializeField] private string _playButtonText = "Putar Intro";
        [SerializeField] private string _skipButtonText = "Lewati >>";

        [Header("Timeout")]
        [SerializeField] private float _prepareTimeoutSeconds = 10f;

        private VisualElement _playContainer = null!;
        private VisualElement _videoContainer = null!;
        private VisualElement _videoFrame = null!;
        private Label _titleLabel = null!;
        private Label _subtitleLabel = null!;
        private Button _playButton = null!;
        private Button _skipButton = null!;

        private RenderTexture _renderTexture = null!;

        private void Awake()
        {
            GameLogger.Info("[VideoPlayerUI] Awake called");

            if (_videoPlayer == null)
                _videoPlayer = GetComponent<VideoPlayer>();
        }

        private void OnEnable()
        {
            GameLogger.Info("[VideoPlayerUI] OnEnable called");
            EnsureDocument();
            BindUI();
            SetupVideoPlayer();
            ShowPlayScreen();
        }

        private void OnDisable()
        {
            GameLogger.Info("[VideoPlayerUI] OnDisable called");
            UnbindUI();
            CleanupVideoPlayer();

            if (_videoFrame != null)
                _videoFrame.style.backgroundImage = StyleKeyword.Null;
        }

        private void OnDestroy()
        {
            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
                _renderTexture = null;
            }
        }

        private void EnsureDocument()
        {
            if (_document == null) return;

            if (_document.visualTreeAsset == null && _visualTreeAsset != null)
                _document.visualTreeAsset = _visualTreeAsset;

            if (_styleSheet != null && _document.rootVisualElement != null
                && !_document.rootVisualElement.styleSheets.Contains(_styleSheet))
            {
                _document.rootVisualElement.styleSheets.Add(_styleSheet);
            }
        }

        private void BindUI()
        {
            if (_document == null || _document.rootVisualElement == null) return;

            var root = _document.rootVisualElement;
            _playContainer = root.Q<VisualElement>(UIConstants.Video.PlayContainer);
            _videoContainer = root.Q<VisualElement>(UIConstants.Video.VideoContainer);
            _videoFrame = root.Q<VisualElement>(UIConstants.Video.VideoFrame);
            _titleLabel = root.Q<Label>(UIConstants.Video.TitleLabel);
            _subtitleLabel = root.Q<Label>(UIConstants.Video.SubtitleLabel);
            _playButton = root.Q<Button>(UIConstants.Video.PlayButton);
            _skipButton = root.Q<Button>(UIConstants.Video.SkipButton);

            if (_titleLabel != null) _titleLabel.text = _titleText;
            if (_subtitleLabel != null) _subtitleLabel.text = _subtitleText;
            if (_playButton != null) _playButton.text = _playButtonText;
            if (_skipButton != null) _skipButton.text = _skipButtonText;

            if (_playButton != null) _playButton.clicked += OnPlayClicked;
            if (_skipButton != null) _skipButton.clicked += OnSkipClicked;
        }

        private void UnbindUI()
        {
            if (_playButton != null) _playButton.clicked -= OnPlayClicked;
            if (_skipButton != null) _skipButton.clicked -= OnSkipClicked;

            _playContainer = null;
            _videoContainer = null;
            _videoFrame = null;
            _titleLabel = null;
            _subtitleLabel = null;
            _playButton = null;
            _skipButton = null;
        }

        private void SetupVideoPlayer()
        {
            if (_videoPlayer == null)
            {
                GameLogger.Error("[VideoPlayerUI] VideoPlayer is null in SetupVideoPlayer!");
                return;
            }

            EnsureRenderTexture();

            _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            _videoPlayer.targetTexture = _renderTexture;
            _videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
            _videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            _videoPlayer.skipOnDrop = true;
            _videoPlayer.waitForFirstFrame = false;

            _videoPlayer.errorReceived += OnVideoError;
            _videoPlayer.loopPointReached += OnVideoFinished;
            _videoPlayer.prepareCompleted += OnVideoPrepared;

            GameLogger.Info($"[VideoPlayerUI] SetupVideoPlayer: renderMode={_videoPlayer.renderMode}, audioOutputMode={_videoPlayer.audioOutputMode}");

#if UNITY_WEBGL && !UNITY_EDITOR
            SetupWebGLSource();
#else
            SetupEditorSource();
#endif
        }

        private void EnsureRenderTexture()
        {
            if (_renderTexture != null) return;

            _renderTexture = new RenderTexture(_renderTextureSize.x, _renderTextureSize.y, 0, RenderTextureFormat.ARGB32)
            {
                name = "VideoPlayerUI_RT",
                antiAliasing = 1,
                useMipMap = false,
                autoGenerateMips = false
            };
            _renderTexture.Create();
        }

        private void OnVideoPrepared(VideoPlayer vp)
        {
            GameLogger.Info("[VideoPlayerUI] prepareCompleted: binding video texture");
            if (_videoFrame == null || _renderTexture == null) return;

            _videoFrame.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(_renderTexture));
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private void SetupWebGLSource()
        {
            string fileName = GameServices.Video?.IntroVideoFileName ?? "Videos/Intro.webm";
            string url = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
            _videoPlayer.source = VideoSource.Url;
            _videoPlayer.url = url;
            GameLogger.Info($"[VideoPlayerUI] WebGL mode: URL={url}");
        }
#else
        private void SetupEditorSource()
        {
            var globalClip = GameServices.Video?.GlobalIntroVideo;
            if (globalClip != null)
            {
                _videoPlayer.source = VideoSource.VideoClip;
                _videoPlayer.clip = globalClip;
                GameLogger.Info($"[VideoPlayerUI] Editor mode: VideoClip={_videoPlayer.clip.name}");
            }
            else
            {
                string fileName = GameServices.Video?.IntroVideoFileName ?? "Videos/Intro.webm";
                string url = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
                _videoPlayer.source = VideoSource.Url;
                _videoPlayer.url = url;
                GameLogger.Info($"[VideoPlayerUI] Editor mode (no clip): URL={url}");
            }
        }
#endif

        private void CleanupVideoPlayer()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.errorReceived -= OnVideoError;
                _videoPlayer.loopPointReached -= OnVideoFinished;
                _videoPlayer.prepareCompleted -= OnVideoPrepared;
                _videoPlayer.Stop();
            }
        }

        public void ShowPlayScreen()
        {
            if (_playContainer != null) _playContainer.style.display = DisplayStyle.Flex;
            if (_videoContainer != null) _videoContainer.style.display = DisplayStyle.None;
        }

        private void ShowVideoScreen()
        {
            if (_playContainer != null) _playContainer.style.display = DisplayStyle.None;
            if (_videoContainer != null) _videoContainer.style.display = DisplayStyle.Flex;
        }

        private void OnPlayClicked()
        {
            GameLogger.Info("[VideoPlayerUI] Play button clicked");

            if (_videoPlayer == null)
            {
                GameLogger.Warn("[VideoPlayerUI] No VideoPlayer, skipping to investigation");
                OnVideoFinishedOrSkipped();
                return;
            }

            bool hasSource = _videoPlayer.source == VideoSource.VideoClip
                ? _videoPlayer.clip != null
                : !string.IsNullOrEmpty(_videoPlayer.url);

            if (!hasSource)
            {
                GameLogger.Warn("[VideoPlayerUI] No video source assigned, skipping to investigation");
                OnVideoFinishedOrSkipped();
                return;
            }

            GameLogger.Info($"[VideoPlayerUI] Source: {_videoPlayer.source}, " +
                $"Clip={(_videoPlayer.clip != null ? _videoPlayer.clip.name : "null")}, " +
                $"URL={(_videoPlayer.url ?? "null")}");

            ShowVideoScreen();
            StartCoroutine(PrepareAndPlay());
        }

        private IEnumerator PrepareAndPlay()
        {
            GameLogger.Info("[VideoPlayerUI] PrepareAndPlay: starting Prepare()");
            _videoPlayer.Prepare();

            if (_videoPlayer.isPrepared)
            {
                GameLogger.Info("[VideoPlayerUI] Video already prepared, playing immediately");
            }
            else
            {
                float elapsed = 0f;
                while (!_videoPlayer.isPrepared)
                {
                    if (_videoPlayer == null)
                    {
                        GameLogger.Warn("[VideoPlayerUI] VideoPlayer destroyed during prepare");
                        yield break;
                    }

                    elapsed += Time.unscaledDeltaTime;
                    if (elapsed > _prepareTimeoutSeconds)
                    {
                        GameLogger.Warn($"[VideoPlayerUI] Prepare timeout after {_prepareTimeoutSeconds}s, attempting Play() anyway");
                        break;
                    }

                    yield return null;
                }

                if (_videoPlayer == null) yield break;

                GameLogger.Info($"[VideoPlayerUI] Prepare completed in {elapsed:F2}s, isPrepared={_videoPlayer.isPrepared}");
            }

            _videoPlayer.Play();
            GameLogger.Info("[VideoPlayerUI] Play() called");
        }

        private void OnSkipClicked()
        {
            GameLogger.Info("[VideoPlayerUI] Skip button clicked");
            StopVideo();
        }

        private void OnVideoFinished(VideoPlayer vp)
        {
            GameLogger.Info("[VideoPlayerUI] Video finished playing");
            OnVideoFinishedOrSkipped();
        }

        private void OnVideoError(VideoPlayer vp, string message)
        {
            GameLogger.Error($"[VideoPlayerUI] Video error: {message}");
            OnVideoFinishedOrSkipped();
        }

        private void StopVideo()
        {
            StopAllCoroutines();

            if (_videoPlayer != null)
                _videoPlayer.Stop();

            if (_videoFrame != null)
                _videoFrame.style.backgroundImage = StyleKeyword.Null;

            OnVideoFinishedOrSkipped();
        }

        private void OnVideoFinishedOrSkipped()
        {
            ShowPlayScreen();
            GameServices.UI?.HideAllPanels();
            GameServices.GameState?.SetState(GameState.Investigation);
        }
    }
}
