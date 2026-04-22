using UnityEngine;

namespace CriminalCase2.Data
{
    [CreateAssetMenu(fileName = "ClueData", menuName = "CriminalCase2/Clue Data")]
    public class ClueData : ScriptableObject
    {
        [Header("Display")]
        [SerializeField] private string _clueName;
        [SerializeField] [TextArea] private string _description;

        [Header("Visuals")]
        [SerializeField] private Sprite _clueSprite;
        [SerializeField] private Sprite _clueIcon;

        [Header("Gameplay")]
        [SerializeField] private SuspectData _linkedSuspect;

        public string ClueName => _clueName;
        public string Description => _description;
        public Sprite ClueSprite => _clueSprite;
        public Sprite ClueIcon => _clueIcon;
        public SuspectData LinkedSuspect => _linkedSuspect;
    }
}