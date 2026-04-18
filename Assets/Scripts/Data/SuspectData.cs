using UnityEngine;
using CriminalCase2.Data;

namespace CriminalCase2.Data
{
    [CreateAssetMenu(fileName = "SuspectData", menuName = "CriminalCase2/Suspect Data")]
    public class SuspectData : ScriptableObject
    {
        [Header("Display")]
        [SerializeField] private string _suspectName;
        [SerializeField] private Texture2D _portrait;

        [Header("Description & Evidence")]
        [SerializeField] [TextArea] private string _description;
        [SerializeField] [TextArea] private string _clueHint;
        [SerializeField] [TextArea] private string _evidenceText;
        [SerializeField] private Texture2D _evidenceImage;

        [Header("Answers")]
        [SerializeField] private DrugTestResult _drugTestResult;
        [SerializeField] private SuspectRole _correctRole;

        [Header("Feedback")]
        [SerializeField] [TextArea] private string _feedbackTextCorrect;
        [SerializeField] [TextArea] private string _feedbackTextWrong;

        public string SuspectName => _suspectName;
        public Texture2D Portrait => _portrait;
        public string Description => _description;
        public string ClueHint => _clueHint;
        public string EvidenceText => _evidenceText;
        public Texture2D EvidenceImage => _evidenceImage;
        public DrugTestResult DrugTestResult => _drugTestResult;
        public SuspectRole CorrectRole => _correctRole;
        public string FeedbackTextCorrect => _feedbackTextCorrect;
        public string FeedbackTextWrong => _feedbackTextWrong;
    }
}
