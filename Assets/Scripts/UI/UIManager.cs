using UnityEngine;
using UnityEngine.UIElements;
using CriminalCase2.Data;
using CriminalCase2.Managers;

namespace CriminalCase2.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private UIDocument _videoPlayerPanel;
        [SerializeField] private UIDocument _tutorialPanel;
        [SerializeField] private UIDocument _suspectDetailPanel;
        [SerializeField] private UIDocument _resultPanel;

        private VideoPlayerUI _videoPlayerUI;
        private TutorialUI _tutorialUI;
        private SuspectDetailUI _suspectDetailUI;
        private ResultUI _resultUI;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            AutoFindPanels();
            LoadUXMLAssets();
        }

        private void AutoFindPanels()
        {
            var documents = GetComponentsInChildren<UIDocument>();
            if (documents.Length > 0) _videoPlayerPanel = documents[0];
            if (documents.Length > 1) _tutorialPanel = documents[1];
            if (documents.Length > 2) _suspectDetailPanel = documents[2];
            if (documents.Length > 3) _resultPanel = documents[3];
        }

        private void LoadUXMLAssets()
        {
            if (_videoPlayerPanel != null)
            {
                var videoAsset = Resources.Load<VisualTreeAsset>("UI/VideoPanel");
                if (videoAsset != null) _videoPlayerPanel.visualTreeAsset = videoAsset;
            }
            if (_tutorialPanel != null)
            {
                var tutorialAsset = Resources.Load<VisualTreeAsset>("UI/TutorialPanel");
                if (tutorialAsset != null) _tutorialPanel.visualTreeAsset = tutorialAsset;
            }
            if (_suspectDetailPanel != null)
            {
                var suspectAsset = Resources.Load<VisualTreeAsset>("UI/SuspectDetailPanel");
                if (suspectAsset != null) _suspectDetailPanel.visualTreeAsset = suspectAsset;
            }
            if (_resultPanel != null)
            {
                var resultAsset = Resources.Load<VisualTreeAsset>("UI/ResultPanel");
                if (resultAsset != null) _resultPanel.visualTreeAsset = resultAsset;
            }
        }

        private void OnEnable()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            if (_videoPlayerUI == null && _videoPlayerPanel != null)
                _videoPlayerUI = _videoPlayerPanel.GetComponent<VideoPlayerUI>();

            if (_tutorialUI == null && _tutorialPanel != null)
                _tutorialUI = _tutorialPanel.GetComponent<TutorialUI>();

            if (_suspectDetailUI == null && _suspectDetailPanel != null)
                _suspectDetailUI = _suspectDetailPanel.GetComponent<SuspectDetailUI>();

            if (_resultUI == null && _resultPanel != null)
                _resultUI = _resultPanel.GetComponent<ResultUI>();
        }

        public void ShowVideoPlayer()
        {
            HideAllPanels();
            SetPanelActive(_videoPlayerPanel, true);
        }

        public void ShowTutorial()
        {
            HideAllPanels();
            SetPanelActive(_tutorialPanel, true);
        }

        public void ShowSuspectDetail(SuspectData suspect)
        {
            HideAllPanels();
            SetPanelActive(_suspectDetailPanel, true);
            _suspectDetailUI?.Populate(suspect);
        }

        public void ShowResults()
        {
            HideAllPanels();
            SetPanelActive(_resultPanel, true);
            _resultUI?.Populate(GameManager.Instance.VerdictRecords);
        }

        public void HideAllPanels()
        {
            SetPanelActive(_videoPlayerPanel, false);
            SetPanelActive(_tutorialPanel, false);
            SetPanelActive(_suspectDetailPanel, false);
            SetPanelActive(_resultPanel, false);
        }

        private void SetPanelActive(UIDocument panel, bool active)
        {
            if (panel != null)
            {
                panel.rootVisualElement.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void OnDisable()
        {
            _videoPlayerUI = null;
            _tutorialUI = null;
            _suspectDetailUI = null;
            _resultUI = null;
        }
    }
}
