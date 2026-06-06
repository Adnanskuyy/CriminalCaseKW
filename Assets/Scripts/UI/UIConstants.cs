namespace CriminalCase2.UI
{
    /// <summary>
    /// Centralized UXML element name constants. Every <c>name="..."</c> attribute
    /// in a UXML asset under <c>Assets/UI/UXML/</c> must have a matching
    /// constant here. Call sites use these instead of inline string literals
    /// so renames stay in sync and the compiler catches typos.
    /// </summary>
    public static class UIConstants
    {
        public static class Tutorial
        {
            public const string CloseButton = "tutorial-close-button";
            public const string ReplayVideoButton = "tutorial-replay-video-button";
        }

        public static class SuspectDetail
        {
            public const string SuspectNameLabel = "suspect-name-label";
            public const string DescriptionLabel = "description-label";
            public const string EvidenceTextLabel = "evidence-text-label";
            public const string DrugTestResultLabel = "drug-test-result-label";
            public const string DrugTestButton = "drug-test-button";
            public const string VerdictUserButton = "verdict-user-button";
            public const string VerdictDealerButton = "verdict-dealer-button";
            public const string VerdictNormalButton = "verdict-normal-button";
            public const string CloseButton = "detail-close-button";
        }

        public static class StatusHud
        {
            public const string Button = "status-hud-button";
        }

        public static class Result
        {
            public const string ResultsContainer = "results-container";
            public const string NextLevelButton = "next-level-button";
        }

        public static class CheckStatus
        {
            public const string Container = "check-status-container";
            public const string Empty = "check-status-empty";
            public const string CloseButton = "check-status-close-button";
            public const string CheckResultButton = "check-result-button";
        }

        public static class Video
        {
            public const string PlayContainer = "play-container";
            public const string VideoContainer = "video-container";
            public const string VideoFrame = "video-frame";
            public const string TitleLabel = "title-label";
            public const string SubtitleLabel = "subtitle-label";
            public const string PlayButton = "play-button";
            public const string SkipButton = "skip-button";
        }
    }
}
