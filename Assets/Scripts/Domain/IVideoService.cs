using UnityEngine.Video;

namespace CriminalCase2.Domain
{
    public interface IVideoService
    {
        VideoClip? GlobalIntroVideo { get; }
        string? IntroVideoFileName { get; }
    }
}
