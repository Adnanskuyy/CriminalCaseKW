using UnityEngine;
using UnityEngine.UIElements;
using CriminalCase2.Data;
using CriminalCase2.Managers;

namespace CriminalCase2.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Video Panel (UGUI)")]
        [SerializeField] private GameObject _videoPlayerPanel;

        [Header("UI Toolkit Panels")]
        [SerializeField] private UIDocument _tutorialPanel;
        [SerializeField] private UIDocument _suspectDetailPanel;
        [SerializeField] private UIDocument _checkStatusPanel;
        [SerializeField] private UIDocument _resultPanel;
        [SerializeField] private UIDocument _statusHUD;
        [SerializeField] private UIDocument _clueSearchPanel;

        private VideoPlayerUI _videoPlayerUI;
        private TutorialUI _tutorialUI;
        private SuspectDetailUI _suspectDetailUI;
        private CheckStatusUI _checkStatusUI;
        private ResultUI _resultUI;
        private StatusHUD _statusHUDUI;
        private ClueSearchUI _clueSearchUI;

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
            if (_videoPlayerPanel == null)
            {
                var vui = GetComponentInChildren<VideoPlayerUI>();
                if (vui != null)
                    _videoPlayerPanel = vui.gameObject;
            }

            var documents = GetComponentsInChildren<UIDocument>();
            if (_tutorialPanel == null && documents.Length > 0) _tutorialPanel = documents[0];
            if (_suspectDetailPanel == null && documents.Length > 1) _suspectDetailPanel = documents[1];
            if (_checkStatusPanel == null && documents.Length > 2) _checkStatusPanel = documents[2];
            if (_resultPanel == null && documents.Length > 3) _resultPanel = documents[3];
            if (_statusHUD == null && documents.Length > 4) _statusHUD = documents[4];
            if (_clueSearchPanel == null && documents.Length > 5) _clueSearchPanel = documents[5];
        }

        private void InitializePanels()
        {
            var commonStyle = Resources.Load<StyleSheet>("UI/Common");

            InitializeUIToolkitPanel(_tutorialPanel, "UI/TutorialPanel", commonStyle);
            InitializeUIToolkitPanel(_suspectDetailPanel, "UI/SuspectDetailPanel", commonStyle);
            InitializeUIToolkitPanel(_checkStatusPanel, "UI/CheckStatusPanel", commonStyle);
            InitializeUIToolkitPanel(_resultPanel, "UI/ResultPanel", commonStyle);
            InitializeUIToolkitPanel(_statusHUD, "UI/StatusHUD", commonStyle);
            InitializeUIToolkitPanel(_clueSearchPanel, "UI/ClueSearchPanel", commonStyle);

            if (_videoPlayerUI == null && _videoPlayerPanel != null)
                _videoPlayerUI = _videoPlayerPanel.GetComponent<VideoPlayerUI>();

            if (_tutorialUI == null && _tutorialPanel != null)
                _tutorialUI = _tutorialPanel.GetComponent<TutorialUI>();

            if (_suspectDetailUI == null && _suspectDetailPanel != null)
                _suspectDetailUI = _suspectDetailPanel.GetComponent<SuspectDetailUI>();

            if (_checkStatusUI == null && _checkStatusPanel != null)
                _checkStatusUI = _checkStatusPanel.GetComponent<CheckStatusUI>();

            if (_resultUI == null && _resultPanel != null)
                _resultUI = _resultPanel.GetComponent<ResultUI>();

            if (_statusHUDUI == null && _statusHUD != null)
                _statusHUDUI = _statusHUD.GetComponent<StatusHUD>();

            if (_clueSearchUI == null && _clueSearchPanel != null)
                _clueSearchUI = _clueSearchPanel.GetComponent<ClueSearchUI>();
        }

        private void InitializeUIToolkitPanel(UIDocument panel, string resourcePath, StyleSheet commonStyle)
        {
            if (panel == null) return;

            if (panel.visualTreeAsset == null)
            {
                var uxml = Resources.Load<VisualTreeAsset>(resourcePath);
                if (uxml == null)
                {
                    Debug.LogWarning($"[UIManager] Failed to load UXML from Resources/{resourcePath}.uxml");
                    return;
                }
                panel.visualTreeAsset = uxml;
            }

            if (commonStyle != null && panel.rootVisualElement != null)
            {
                panel.rootVisualElement.styleSheets.Add(commonStyle);
            }
        }

        public void ShowVideoPlayer()
        {
            if (_videoPlayerPanel != null && _videoPlayerPanel.activeSelf)
                return;

            HideAllPanels();
            if (_videoPlayerPanel != null)
            {
                _videoPlayerPanel.SetActive(true);
                _videoPlayerUI?.ShowPlayScreen();
            }
        }

        public void ShowTutorial()
        {
            HideAllPanels();
            SetUIToolkitPanelActive(_tutorialPanel, true);
        }

        public void ShowSuspectDetail(SuspectData suspect)
        {
            HideAllPanels();
            SetUIToolkitPanelActive(_suspectDetailPanel, true);
            _suspectDetailUI?.Populate(suspect);
        }

        public void ShowCheckStatus()
        {
            HideAllPanels();
            SetUIToolkitPanelActive(_checkStatusPanel, true);
            _checkStatusUI?.Populate(GameManager.Instance.VerdictRecords);
        }

        public void HideCheckStatus()
        {
            SetUIToolkitPanelActive(_checkStatusPanel, false);
        }

        public void ShowResults()
        {
            HideAllPanels();
            HideStatusHUD();
            SetUIToolkitPanelActive(_resultPanel, true);
            _resultUI?.Populate(GameManager.Instance.VerdictRecords);
        }

        public void ShowStatusHUD()
        {
            if (_statusHUDUI == null) return;
            
            // Only show StatusHUD during Deduction or Results phase
            if (GameManager.Instance != null && 
                GameManager.Instance.CurrentState != GameState.Deduction &&
                GameManager.Instance.CurrentState != GameState.Results)
            {
                return;
            }
            
            SetUIToolkitPanelActive(_statusHUD, true);
            _statusHUDUI.Initialize();
        }

        public void HideStatusHUD()
        {
            SetUIToolkitPanelActive(_statusHUD, false);
        }

        public void UpdateStatusHUD()
        {
            _statusHUDUI?.UpdateButtonText();
        }

        public void ShowClueSearch()
        {
            Debug.Log("[UIManager] ShowClueSearch() called.");
            if (ClueManager.Instance != null && GameManager.Instance.CurrentLevel != null)
            {
                Debug.Log("[UIManager] Initializing ClueSearchUI...");
                _clueSearchUI?.Initialize(GameManager.Instance.CurrentLevel.Clues);
            }
            Debug.Log("[UIManager] Activating ClueSearchPanel GameObject...");
            SetUIToolkitPanelActive(_clueSearchPanel, true);
        }

        public void HideClueSearch()
        {
            SetUIToolkitPanelActive(_clueSearchPanel, false);
        }

        public void HideAllPanels()
        {
            if (_videoPlayerPanel != null)
                _videoPlayerPanel.SetActive(false);
            SetUIToolkitPanelActive(_tutorialPanel, false);
            SetUIToolkitPanelActive(_suspectDetailPanel, false);
            SetUIToolkitPanelActive(_checkStatusPanel, false);
            SetUIToolkitPanelActive(_resultPanel, false);
            SetUIToolkitPanelActive(_clueSearchPanel, false);
        }

        private void SetUIToolkitPanelActive(UIDocument panel, bool active)
        {
            if (panel == null) return;
            
            // Enable/disable the entire GameObject, not just the visual element
            // This prevents OnEnable() from firing on hidden panels and blocking input
            panel.gameObject.SetActive(active);
        }

        private void OnDisable()
        {
            _videoPlayerUI = null;
            _tutorialUI = null;
            _suspectDetailUI = null;
            _checkStatusUI = null;
            _resultUI = null;
            _statusHUDUI = null;
            _clueSearchUI = null;
        }
    }
}