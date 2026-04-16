using UnityEngine;
using UnityEngine.UIElements;
using CriminalCase2.Data;
using CriminalCase2.Managers;
using CriminalCase2.Services;
using CriminalCase2.Services.Interfaces;
using CriminalCase2.Utils;

namespace CriminalCase2.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Video Panel (UGUI)")]
        [SerializeField] private GameObject _videoPlayerPanel;

        [Header("UI Toolkit Panels")]
        [SerializeField] private UIDocument _suspectDetailPanel;
        [SerializeField] private UIDocument _checkStatusPanel;
        [SerializeField] private UIDocument _resultPanel;
        [SerializeField] private UIDocument _statusHUD;
        [SerializeField] private UIDocument _clueSearchPanel;
        [SerializeField] private UIDocument _clueMatchingPanel;

        private VideoPlayerUI _videoPlayerUI;
        private SuspectDetailUI _suspectDetailUI;
        private CheckStatusUI _checkStatusUI;
        private ResultUI _resultUI;
        private StatusHUD _statusHUDUI;
        private ClueSearchUI _clueSearchUI;
        private ClueMatchingUI _clueMatchingUI;

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
            
            // Ensure services are available
            EnsureServicesInitialized();
            
            // Show correct panel based on current game state
            ShowCorrectPanelForCurrentState();
        }

        private void ShowCorrectPanelForCurrentState()
        {
            if (GameManager.Instance == null)
            {
                LoggingUtility.Warning("UIManager", "GameManager.Instance is null, cannot determine initial state");
                return;
            }

            GameState currentState = GameManager.Instance.CurrentState;
            LoggingUtility.LogUI($"Initializing UI for state: {currentState}");

            switch (currentState)
            {
                case GameState.IntroVideo:
                    ShowVideoPlayer();
                    break;
                case GameState.ClueSearch:
                    HideAllPanels();
                    ShowClueSearch();
                    break;
                case GameState.ClueMatching:
                    HideAllPanels();
                    ShowClueMatching();
                    break;
                case GameState.RoleAssignment:
                    HideAllPanels();
                    ShowStatusHUD();
                    break;
                case GameState.Results:
                    HideAllPanels();
                    ShowResults();
                    break;
                default:
                    HideAllPanels();
                    break;
            }
        }

        private void EnsureServicesInitialized()
        {
            // Ensure ClueService is registered
            if (!ServiceLocator.IsRegistered<IClueService>())
            {
                var clueService = new ClueService();
                ServiceLocator.Register<IClueService>(clueService);
                LoggingUtility.LogDebug("UIManager", "ClueService registered");
            }

            // Ensure GameStateService is registered
            if (!ServiceLocator.IsRegistered<IGameStateService>())
            {
                var gameStateService = new GameStateService();
                ServiceLocator.Register<IGameStateService>(gameStateService);
                LoggingUtility.LogDebug("UIManager", "GameStateService registered");
            }

            // Ensure VideoPlayerService is registered
            if (!ServiceLocator.IsRegistered<IVideoPlayerService>())
            {
                var videoService = FindFirstObjectByType<VideoPlayerService>();
                if (videoService == null)
                {
                    var go = new GameObject("VideoPlayerService");
                    videoService = go.AddComponent<VideoPlayerService>();
                    DontDestroyOnLoad(go);
                }
                ServiceLocator.Register<IVideoPlayerService>(videoService);
                LoggingUtility.LogDebug("UIManager", "VideoPlayerService registered");
            }
        }

        private void AutoFindPanels()
        {
            if (_videoPlayerPanel == null)
            {
                var vui = GetComponentInChildren<VideoPlayerUI>(true);
                if (vui != null)
                    _videoPlayerPanel = vui.gameObject;
            }

            var documents = GetComponentsInChildren<UIDocument>(true);
            if (_suspectDetailPanel == null && documents.Length > 0) _suspectDetailPanel = documents[0];
            if (_checkStatusPanel == null && documents.Length > 1) _checkStatusPanel = documents[1];
            if (_resultPanel == null && documents.Length > 2) _resultPanel = documents[2];
            if (_statusHUD == null && documents.Length > 3) _statusHUD = documents[3];
            if (_clueSearchPanel == null && documents.Length > 4) _clueSearchPanel = documents[4];
            if (_clueMatchingPanel == null && documents.Length > 5) _clueMatchingPanel = documents[5];
        }

        private void InitializePanels()
        {
            var commonStyle = Resources.Load<StyleSheet>("UI/Common");

            InitializeUIToolkitPanel(_suspectDetailPanel, "UI/SuspectDetailPanel", commonStyle);
            InitializeUIToolkitPanel(_checkStatusPanel, "UI/CheckStatusPanel", commonStyle);
            InitializeUIToolkitPanel(_resultPanel, "UI/ResultPanel", commonStyle);
            InitializeUIToolkitPanel(_statusHUD, "UI/StatusHUD", commonStyle);
            InitializeUIToolkitPanel(_clueSearchPanel, "UI/ClueSearchPanel", commonStyle);
            InitializeUIToolkitPanel(_clueMatchingPanel, "UI/ClueMatchingPanel", commonStyle);

            if (_videoPlayerUI == null && _videoPlayerPanel != null)
                _videoPlayerUI = _videoPlayerPanel.GetComponent<VideoPlayerUI>();

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

            if (_clueMatchingUI == null && _clueMatchingPanel != null)
                _clueMatchingUI = _clueMatchingPanel.GetComponent<ClueMatchingUI>();
        }

        private void InitializeUIToolkitPanel(UIDocument panel, string resourcePath, StyleSheet commonStyle)
        {
            if (panel == null) return;

            if (panel.visualTreeAsset == null)
            {
                var uxml = Resources.Load<VisualTreeAsset>(resourcePath);
                if (uxml == null)
                {
                    LoggingUtility.Warning("UIManager", $"Failed to load UXML from Resources/{resourcePath}.uxml");
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
                LoggingUtility.LogUI("VideoPlayerPanel shown");
            }
            else
            {
                LoggingUtility.Error("UIManager", "VideoPlayerPanel reference is null!");
            }
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
            
            // Only show StatusHUD during RoleAssignment or Results phase
            if (GameManager.Instance != null && 
                GameManager.Instance.CurrentState != GameState.RoleAssignment &&
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
            LoggingUtility.LogUI("ShowClueSearch() called.");

            // Activate panel FIRST so VisualTree is ready before Initialize
            LoggingUtility.LogUI("Activating ClueSearchPanel GameObject...");
            SetUIToolkitPanelActive(_clueSearchPanel, true);

            // THEN initialize after panel is active
            var clueService = ServiceLocator.Get<IClueService>();
            if (clueService != null && GameManager.Instance?.CurrentLevel != null)
            {
                LoggingUtility.LogUI("Initializing ClueSearchUI...");
                clueService.Initialize(GameManager.Instance.CurrentLevel.Clues);
                _clueSearchUI?.Initialize(GameManager.Instance.CurrentLevel.Clues);
            }
        }

        public void HideClueSearch()
        {
            SetUIToolkitPanelActive(_clueSearchPanel, false);
        }

        public void ShowClueMatching()
        {
            LoggingUtility.LogUI("ShowClueMatching() called.");

            SetUIToolkitPanelActive(_clueMatchingPanel, true);

            if (GameManager.Instance?.CurrentLevel != null)
            {
                var level = GameManager.Instance.CurrentLevel;
                _clueMatchingUI?.Initialize(level.Clues, level.Suspects);
            }
        }

        public void HideClueMatching()
        {
            SetUIToolkitPanelActive(_clueMatchingPanel, false);
        }

        public void HideAllPanels()
        {
            if (_videoPlayerPanel != null)
                _videoPlayerPanel.SetActive(false);
            SetUIToolkitPanelActive(_suspectDetailPanel, false);
            SetUIToolkitPanelActive(_checkStatusPanel, false);
            SetUIToolkitPanelActive(_resultPanel, false);
            SetUIToolkitPanelActive(_clueSearchPanel, false);
            SetUIToolkitPanelActive(_clueMatchingPanel, false);
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
            _suspectDetailUI = null;
            _checkStatusUI = null;
            _resultUI = null;
            _statusHUDUI = null;
            _clueSearchUI = null;
            _clueMatchingUI = null;
        }
    }
}
