using UnityEngine;
using CriminalCase2.Data;

namespace CriminalCase2.Data
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "CriminalCase2/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Level Info")]
        [SerializeField] private int _levelIndex;
        [SerializeField] private string _levelName;

        [Header("Visuals")]
        [SerializeField] private Sprite _backgroundSprite;

        [Header("Suspects")]
        [SerializeField] private SuspectData[] _suspects;

        [Header("Mechanics")]
        [SerializeField] private int _maxDrugTestsPerLevel = 2;

        public int LevelIndex => _levelIndex;
        public string LevelName => _levelName;
        public Sprite BackgroundSprite => _backgroundSprite;
        public SuspectData[] Suspects => _suspects;
        public int MaxDrugTestsPerLevel => _maxDrugTestsPerLevel;

        private void OnValidate()
        {
            if (_suspects != null && _suspects.Length != 4)
            {
                Debug.LogWarning($"LevelConfig '{name}' should have exactly 4 suspects, found {_suspects.Length}.");
            }
        }
    }
}
