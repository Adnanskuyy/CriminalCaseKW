using System;
using UnityEngine;
using UnityEngine.Video;

namespace CriminalCase2.Services.Interfaces
{
    /// <summary>
    /// Service interface for video playback functionality.
    /// Abstracts video player implementation from UI components.
    /// </summary>
    public interface IVideoPlayerService
    {
        /// <summary>
        /// Event fired when video starts playing.
        /// </summary>
        event Action OnVideoStarted;

        /// <summary>
        /// Event fired when video finishes playing naturally.
        /// </summary>
        event Action OnVideoFinished;

        /// <summary>
        /// Event fired when video is skipped by user.
        /// </summary>
        event Action OnVideoSkipped;

        /// <summary>
        /// Event fired when an error occurs during playback.
        /// </summary>
        event Action<string> OnVideoError;

        /// <summary>
        /// Event fired when video preparation completes.
        /// </summary>
        event Action OnVideoPrepared;

        /// <summary>
        /// Current playback state.
        /// </summary>
        VideoPlaybackState PlaybackState { get; }

        /// <summary>
        /// Current video texture for rendering.
        /// </summary>
        Texture VideoTexture { get; }

        /// <summary>
        /// Video duration in seconds. Returns 0 if not prepared.
        /// </summary>
        float Duration { get; }

        /// <summary>
        /// Current playback time in seconds.
        /// </summary>
        float CurrentTime { get; }

        /// <summary>
        /// Whether the video is currently playing.
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// Whether the video has been prepared and is ready to play.
        /// </summary>
        bool IsPrepared { get; }

        /// <summary>
        /// Load a video from a VideoClip (Editor/Standalone).
        /// </summary>
        void LoadVideo(VideoClip clip);

        /// <summary>
        /// Load a video from a URL (WebGL/Streaming).
        /// </summary>
        void LoadVideo(string url);

        /// <summary>
        /// Prepare the video for playback. Must be called before Play().
        /// </summary>
        void Prepare();

        /// <summary>
        /// Start video playback.
        /// </summary>
        void Play();

        /// <summary>
        /// Pause video playback.
        /// </summary>
        void Pause();

        /// <summary>
        /// Stop video playback and reset to beginning.
        /// </summary>
        void Stop();

        /// <summary>
        /// Skip the current video.
        /// </summary>
        void Skip();

        /// <summary>
        /// Set playback volume (0-1).
        /// </summary>
        void SetVolume(float volume);

        /// <summary>
        /// Seek to a specific time.
        /// </summary>
        void Seek(float time);

        /// <summary>
        /// Clean up resources and release video player.
        /// </summary>
        void Cleanup();
    }

    /// <summary>
    /// Video playback states.
    /// </summary>
    public enum VideoPlaybackState
    {
        Idle,
        Loading,
        Preparing,
        Ready,
        Playing,
        Paused,
        Finished,
        Error
    }
}
