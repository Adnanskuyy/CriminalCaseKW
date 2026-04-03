using CriminalCase2.Data;

namespace CriminalCase2.Data
{
    public struct VerdictRecord
    {
        public SuspectData Suspect;
        public SuspectRole PlayerChoice;
        public SuspectRole CorrectAnswer;
        public bool IsCorrect;
        public string FeedbackText;

        public VerdictRecord(SuspectData suspect, SuspectRole playerChoice)
        {
            Suspect = suspect;
            PlayerChoice = playerChoice;
            CorrectAnswer = suspect.CorrectRole;
            IsCorrect = playerChoice == suspect.CorrectRole;
            FeedbackText = IsCorrect ? suspect.FeedbackTextCorrect : suspect.FeedbackTextWrong;
        }
    }
}
