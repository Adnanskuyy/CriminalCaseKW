using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using CriminalCase2.Services.Interfaces;
using CriminalCase2.Utils;

namespace CriminalCase2.Services
{
    /// <summary>
    /// Service implementation for video playback.
    /// Handles video loading, preparation, and playback with WebGL support.
    /// </summary>
    public class VideoPlayerService : MonoBehaviour, IVideoPlayerService
    {
        [Header("Configuration")]
        [SerializeField] private float _prepareTimeoutSeconds = 10f;
        [SerializeField] private bool _skipOnDrop = true;
        [SerializeField] private bool _waitForFirstFrame = false;

        private VideoPlayer _videoPlayer;
        private VideoPlaybackState _playbackState = VideoPlaybackState.Idle;
        private Coroutine _prepareCoroutine;

        #region Events

        public event Action OnVideoStarted;
        public event Action OnVideoFinished;
        public event Action OnVideoSkipped;
        public event Action<string> OnVideoError;
        public event Action OnVideoPrepared;

        #endregion

        #region Properties

        public VideoPlaybackState PlaybackState => _playbackState;
        public Texture VideoTexture => _videoPlayer?.texture;
        public float Duration => (float)(_videoPlayer?.length ?? 0.0);
        public float CurrentTime => (float)(_videoPlayer?.time ?? 0.0);
        public bool IsPlaying => _videoPlayer?.isPlaying ?? false;
        public bool IsPrepared => _videoPlayer?.isPrepared ?? false;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeVideoPlayer();
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        #endregion

        private void InitializeVideoPlayer()
        {
            _videoPlayer = gameObject.AddComponent<VideoPlayer>();
            
            // Configure for UI Toolkit rendering (APIOnly mode)
            _videoPlayer.renderMode = VideoRenderMode.APIOnly;
            _videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
            _videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            _videoPlayer.skipOnDrop = _skipOnDrop;
            _videoPlayer.waitForFirstFrame = _waitForFirstFrame;

            // Subscribe to VideoPlayer events
            _videoPlayer.errorReceived += HandleVideoError;
            _videoPlayer.loopPointReached += HandleVideoFinished;
            _videoPlayer.prepareCompleted += HandleVideoPrepared;

            LoggingUtility.LogVideo("VideoPlayer initialized", LogLevel.Debug);
        }

        #region IVideoPlayerService Implementation

        public void LoadVideo(VideoClip clip)
        {
            if (_videoPlayer == null)
            {
                LoggingUtility.Error("Video", "Cannot load video - VideoPlayer is null");
                return;
            }

            if (clip == null)
            {
                LoggingUtility.Error("Video", "Cannot load null VideoClip");
                return;
            }

            Stop();
            _playbackState = VideoPlaybackState.Loading;

            _videoPlayer.source = VideoSource.VideoClip;
            _videoPlayer.clip = clip;

            LoggingUtility.LogVideo($"Loaded VideoClip: {clip.name}");
        }

        public void LoadVideo(string url)
        {
            if (_videoPlayer == null)
            {
                LoggingUtility.Error("Video", "Cannot load video - VideoPlayer is null");
                return;
            }

            if (string.IsNullOrEmpty(url))
            {
                LoggingUtility.Error("Video", "Cannot load video from null/empty URL");
                return;
            }

            Stop();
            _playbackState = VideoPlaybackState.Loading;

            _videoPlayer.source = VideoSource.Url;
            _videoPlayer.url = url;

            LoggingUtility.LogVideo($"Loaded video from URL: {url}");
        }

        public void Prepare()
        {
            if (_videoPlayer == null)
            {
                LoggingUtility.Error("Video", "Cannot prepare - VideoPlayer is null");
                return;
            }

            if (_videoPlayer.isPrepared)
            {
                LoggingUtility.LogVideo("Video already prepared", LogLevel.Debug);
                HandleVideoPrepared(_videoPlayer);
                return;
            }

            _playbackState = VideoPlaybackState.Preparing;

            if (_prepareCoroutine != null)
            {
                StopCoroutine(_prepareCoroutine);
            }

            _prepareCoroutine = StartCoroutine(PrepareCoroutine());
        }

        public void Play()
        {
            if (_videoPlayer == null)
            {
                LoggingUtility.Error("Video", "Cannot play - VideoPlayer is null");
                return;
            }

            if (!_videoPlayer.isPrepared)
            {
                LoggingUtility.Warning("Video", "Video not prepared, calling Prepare() first");
                Prepare();
                return;
            }

            _videoPlayer.Play();
            _playbackState = VideoPlaybackState.Playing;

            LoggingUtility.LogVideo("Video playback started");
            OnVideoStarted?.Invoke();
        }

        public void Pause()
        {
            if (_videoPlayer == null) return;

            _videoPlayer.Pause();
            _playbackState = VideoPlaybackState.Paused;

            LoggingUtility.LogVideo("Video paused", LogLevel.Debug);
        }

        public void Stop()
        {
            if (_prepareCoroutine != null)
            {
                StopCoroutine(_prepareCoroutine);
                _prepareCoroutine = null;
            }

            if (_videoPlayer != null)
            {
                _videoPlayer.Stop();
            }

            _playbackState = VideoPlaybackState.Idle;

            LoggingUtility.LogVideo("Video stopped", LogLevel.Debug);
        }

        public void Skip()
        {
            LoggingUtility.LogVideo("Video skipped by user");
            
            Stop();
            _playbackState = VideoPlaybackState.Finished;
            
            OnVideoSkipped?.Invoke();
        }

        public void SetVolume(float volume)
        {
            if (_videoPlayer == null) return;

            _videoPlayer.SetDirectAudioVolume(0, Mathf.Clamp01(volume));
        }

        public void Seek(float time)
        {
            if (_videoPlayer == null) return;

            _videoPlayer.time = Mathf.Clamp(time, 0, Duration);
        }

        public void Cleanup()
        {
            if (_prepareCoroutine != null)
            {
                StopCoroutine(_prepareCoroutine);
                _prepareCoroutine = null;
            }

            if (_videoPlayer != null)
            {
                _videoPlayer.errorReceived -= HandleVideoError;
                _videoPlayer.loopPointReached -= HandleVideoFinished;
                _videoPlayer.prepareCompleted -= HandleVideoPrepared;

                _videoPlayer.Stop();
                Destroy(_videoPlayer);
                _videoPlayer = null;
            }

            _playbackState = VideoPlaybackState.Idle;

            LoggingUtility.LogVideo("VideoPlayer cleaned up", LogLevel.Debug);
        }

        #endregion

        #region Private Methods

        private IEnumerator PrepareCoroutine()
        {
            LoggingUtility.LogVideo("Starting video preparation");

            _videoPlayer.Prepare();

            float elapsed = 0f;
            while (!_videoPlayer.isPrepared)
            {
                if (_videoPlayer == null)
                {
                    LoggingUtility.Warning("Video", "VideoPlayer destroyed during preparation");
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                if (elapsed > _prepareTimeoutSeconds)
                {
                    LoggingUtility.Warning("Video", $"Video preparation timed out after {_prepareTimeoutSeconds}s");
                    HandleVideoError(_videoPlayer, "Preparation timeout");
                    yield break;
                }

                yield return null;
            }

            LoggingUtility.LogVideo($"Video prepared in {elapsed:F2}s");
        }

        private void HandleVideoPrepared(VideoPlayer vp)
        {
            _playbackState = VideoPlaybackState.Ready;
            _prepareCoroutine = null;

            LoggingUtility.LogVideo("Video preparation completed");
            OnVideoPrepared?.Invoke();
        }

        private void HandleVideoFinished(VideoPlayer vp)
        {
            _playbackState = VideoPlaybackState.Finished;

            LoggingUtility.LogVideo("Video finished playing naturally");
            OnVideoFinished?.Invoke();
        }

        private void HandleVideoError(VideoPlayer vp, string message)
        {
            _playbackState = VideoPlaybackState.Error;

            LoggingUtility.Error("Video", $"Video error: {message}");
            OnVideoError?.Invoke(message);
        }

        #endregion
    }
}
