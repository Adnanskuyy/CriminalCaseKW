namespace CriminalCase2.Data
{
    public struct LevelResult
    {
        public int CorrectClueMatches;
        public int TotalClues;
        public float ClueMatchingAccuracy;
        public int CorrectVerdicts;
        public int TotalSuspects;
        public float VerdictAccuracy;
        public float OverallScore;

        public LevelResult(int correctClueMatches, int totalClues, int correctVerdicts, int totalSuspects)
        {
            CorrectClueMatches = correctClueMatches;
            TotalClues = totalClues;
            ClueMatchingAccuracy = totalClues > 0 ? (float)correctClueMatches / totalClues : 0f;
            CorrectVerdicts = correctVerdicts;
            TotalSuspects = totalSuspects;
            VerdictAccuracy = totalSuspects > 0 ? (float)correctVerdicts / totalSuspects : 0f;
            OverallScore = (ClueMatchingAccuracy + VerdictAccuracy) / 2f;
        }
    }
}
