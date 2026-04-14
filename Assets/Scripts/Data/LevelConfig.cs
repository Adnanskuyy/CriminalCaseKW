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

        [Header("Level Prefab")]
        [Tooltip("The prefab to instantiate for this level")]
        [SerializeField] private GameObject _levelPrefab;

        [Header("Suspects")]
        [SerializeField] private SuspectData[] _suspects;

        [Header("Clues")]
        [SerializeField] private ClueData[] _clues;

        [Header("Mechanics")]
        [SerializeField] private int _maxDrugTestsPerLevel = 2;

        public int LevelIndex => _levelIndex;
        public string LevelName => _levelName;
        public Sprite BackgroundSprite => _backgroundSprite;
        public GameObject LevelPrefab => _levelPrefab;
        public SuspectData[] Suspects => _suspects;
        public ClueData[] Clues => _clues;
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
