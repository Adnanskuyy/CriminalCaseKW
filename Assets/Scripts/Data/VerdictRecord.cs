namespace CriminalCase2.Data
{
    public sealed record VerdictRecord(
        SuspectData Suspect,
        SuspectRole PlayerChoice,
        SuspectRole CorrectAnswer,
        bool IsCorrect,
        string FeedbackText)
    {
        public VerdictRecord(SuspectData suspect, SuspectRole playerChoice)
            : this(
                suspect,
                playerChoice,
                suspect.CorrectRole,
                playerChoice == suspect.CorrectRole,
                playerChoice == suspect.CorrectRole ? suspect.FeedbackTextCorrect : suspect.FeedbackTextWrong)
        { }
    }
}
