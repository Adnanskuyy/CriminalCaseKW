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
        }

        private void Start()
        {
            InitializePanels();
            HideAllPanels();
        }

        private void AutoFindPanels()
        {
            var documents = GetComponentsInChildren<UIDocument>();
            if (_videoPlayerPanel == null && documents.Length > 0) _videoPlayerPanel = documents[0];
            if (_tutorialPanel == null && documents.Length > 1) _tutorialPanel = documents[1];
            if (_suspectDetailPanel == null && documents.Length > 2) _suspectDetailPanel = documents[2];
            if (_resultPanel == null && documents.Length > 3) _resultPanel = documents[3];
        }

        private void InitializePanels()
        {
            var commonStyle = Resources.Load<StyleSheet>("UI/Common");

            InitializePanel(_videoPlayerPanel, "UI/VideoPanel", commonStyle);
            InitializePanel(_tutorialPanel, "UI/TutorialPanel", commonStyle);
            InitializePanel(_suspectDetailPanel, "UI/SuspectDetailPanel", commonStyle);
            InitializePanel(_resultPanel, "UI/ResultPanel", commonStyle);

            if (_videoPlayerUI == null && _videoPlayerPanel != null)
                _videoPlayerUI = _videoPlayerPanel.GetComponent<VideoPlayerUI>();

            if (_tutorialUI == null && _tutorialPanel != null)
                _tutorialUI = _tutorialPanel.GetComponent<TutorialUI>();

            if (_suspectDetailUI == null && _suspectDetailPanel != null)
                _suspectDetailUI = _suspectDetailPanel.GetComponent<SuspectDetailUI>();

            if (_resultUI == null && _resultPanel != null)
                _resultUI = _resultPanel.GetComponent<ResultUI>();
        }

        private void InitializePanel(UIDocument panel, string resourcePath, StyleSheet commonStyle)
        {
            if (panel == null) return;

            var uxml = Resources.Load<VisualTreeAsset>(resourcePath);
            if (uxml == null)
            {
                Debug.LogWarning($"[UIManager] Failed to load UXML from Resources/{resourcePath}.uxml");
                return;
            }

            panel.visualTreeAsset = uxml;

            if (commonStyle != null && panel.rootVisualElement != null)
            {
                panel.rootVisualElement.styleSheets.Add(commonStyle);
            }
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
            if (panel == null) return;
            if (panel.rootVisualElement == null) return;

            var panelElement = panel.rootVisualElement.Q<VisualElement>(className: "panel");
            if (panelElement != null)
            {
                panelElement.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
            }
            else
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
