using UnityEngine;
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

        [Header("UGUI References")]
        [SerializeField] private GameObject _playScreen;
        [SerializeField] private GameObject _videoScreen;
        [SerializeField] private UnityEngine.UI.RawImage _videoRawImage;
        [SerializeField] private UnityEngine.UI.Button _playButton;
        [SerializeField] private UnityEngine.UI.Button _skipButton;
        [SerializeField] private UnityEngine.UI.Text _titleLabel;
        [SerializeField] private UnityEngine.UI.Text _subtitleLabel;

        [Header("Play Screen Text")]
        [SerializeField] private string _titleText = "Criminal Case 2";
        [SerializeField] private string _subtitleText = "Klik di bawah untuk memulai investigasi";
        [SerializeField] private string _playButtonText = "Putar Intro";
        [SerializeField] private string _skipButtonText = "Lewati >>";

        [Header("Timeout")]
        [SerializeField] private float _prepareTimeoutSeconds = 10f;

        private Texture2D _fallbackTexture;
        private bool _isPlaying;

        private void Awake()
        {
            GameLogger.Info("[VideoPlayerUI] Awake called");

            if (_videoPlayer == null)
                _videoPlayer = GetComponent<VideoPlayer>();

            AutoFindReferences();
            LogReferences();
        }

        private void AutoFindReferences()
        {
            if (_playScreen == null)
            {
                var t = transform.Find("PlayScreen");
                if (t != null) { _playScreen = t.gameObject; GameLogger.Info("[VideoPlayerUI] Auto-found PlayScreen"); }
                else GameLogger.Warn("[VideoPlayerUI] Could not find child: PlayScreen");
            }

            if (_videoScreen == null)
            {
                var t = transform.Find("VideoScreen");
                if (t != null) { _videoScreen = t.gameObject; GameLogger.Info("[VideoPlayerUI] Auto-found VideoScreen"); }
                else GameLogger.Warn("[VideoPlayerUI] Could not find child: VideoScreen");
            }

            if (_videoRawImage == null)
            {
                var t = transform.Find("VideoScreen/VideoRawImage");
                if (t != null) { _videoRawImage = t.GetComponent<UnityEngine.UI.RawImage>(); GameLogger.Info($"[VideoPlayerUI] Auto-found VideoRawImage: {_videoRawImage != null}"); }
                else GameLogger.Warn("[VideoPlayerUI] Could not find child: VideoScreen/VideoRawImage");
            }

            if (_playButton == null)
            {
                var t = transform.Find("PlayScreen/PlayButton");
                if (t != null) { _playButton = t.GetComponent<UnityEngine.UI.Button>(); GameLogger.Info($"[VideoPlayerUI] Auto-found PlayButton: {_playButton != null}"); }
                else GameLogger.Warn("[VideoPlayerUI] Could not find child: PlayScreen/PlayButton");
            }

            if (_skipButton == null)
            {
                var t = transform.Find("VideoScreen/SkipButtonContainer/SkipButton");
                if (t != null) { _skipButton = t.GetComponent<UnityEngine.UI.Button>(); GameLogger.Info($"[VideoPlayerUI] Auto-found SkipButton: {_skipButton != null}"); }
                else GameLogger.Warn("[VideoPlayerUI] Could not find child: VideoScreen/SkipButtonContainer/SkipButton");
            }

            if (_titleLabel == null)
            {
                var t = transform.Find("PlayScreen/TitleLabel");
                if (t != null) { _titleLabel = t.GetComponent<UnityEngine.UI.Text>(); }
            }

            if (_subtitleLabel == null)
            {
                var t = transform.Find("PlayScreen/SubtitleLabel");
                if (t != null) { _subtitleLabel = t.GetComponent<UnityEngine.UI.Text>(); }
            }
        }

        private void LogReferences()
        {
            GameLogger.Info($"[VideoPlayerUI] References: " +
                $"VideoPlayer={_videoPlayer != null}, " +
                $"PlayScreen={_playScreen != null}, " +
                $"VideoScreen={_videoScreen != null}, " +
                $"RawImage={_videoRawImage != null}, " +
                $"PlayButton={_playButton != null}, " +
                $"SkipButton={_skipButton != null}");
        }

        private void OnEnable()
        {
            GameLogger.Info("[VideoPlayerUI] OnEnable called");
            SetupVideoPlayer();
            SetupUI();
            ShowPlayScreen();
        }

        private void OnDisable()
        {
            GameLogger.Info("[VideoPlayerUI] OnDisable called");
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

        private void SetupUI()
        {
            if (_titleLabel != null)
                _titleLabel.text = _titleText;
            if (_subtitleLabel != null)
                _subtitleLabel.text = _subtitleText;
            if (_playButton != null)
            {
                var playBtnText = _playButton.GetComponentInChildren<UnityEngine.UI.Text>();
                if (playBtnText != null) playBtnText.text = _playButtonText;
            }
            if (_skipButton != null)
            {
                var skipBtnText = _skipButton.GetComponentInChildren<UnityEngine.UI.Text>();
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
                GameLogger.Error("[VideoPlayerUI] VideoPlayer is null in SetupVideoPlayer!");
                return;
            }

            _videoPlayer.renderMode = VideoRenderMode.APIOnly;
            _videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
            _videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            _videoPlayer.skipOnDrop = true;
            _videoPlayer.waitForFirstFrame = false;

            _videoPlayer.errorReceived += OnVideoError;
            _videoPlayer.loopPointReached += OnVideoFinished;

            GameLogger.Info($"[VideoPlayerUI] SetupVideoPlayer: renderMode={_videoPlayer.renderMode}, audioOutputMode={_videoPlayer.audioOutputMode}");

#if UNITY_WEBGL && !UNITY_EDITOR
            SetupWebGLSource();
#else
            SetupEditorSource();
#endif
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
                _videoPlayer.Stop();
            }
        }

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

            if (_videoRawImage != null && _videoPlayer.texture != null)
            {
                _videoRawImage.color = Color.white;
                _videoRawImage.texture = _videoPlayer.texture;
            }

            _isPlaying = true;
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
            _isPlaying = false;
            OnVideoFinishedOrSkipped();
        }

        private void OnVideoError(VideoPlayer vp, string message)
        {
            GameLogger.Error($"[VideoPlayerUI] Video error: {message}");
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
            GameServices.UI?.HideAllPanels();
            GameServices.GameState?.SetState(GameState.Investigation);
        }

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