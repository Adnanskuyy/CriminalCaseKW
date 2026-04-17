using UnityEngine;
using CriminalCase2.Data;
using CriminalCase2.Services;
using CriminalCase2.Services.Interfaces;
using CriminalCase2.UI;
using CriminalCase2.Utils;

namespace CriminalCase2.Managers
{
    public class GameStateController : MonoBehaviour
    {
        private IGameStateService _stateService;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeStateService();
        }

        private void OnDestroy()
        {
            UnsubscribeFromStateEvents();
        }

        #endregion

        #region Service Initialization

        private void InitializeStateService()
        {
            _stateService = ServiceLocator.Get<IGameStateService>();
            if (_stateService == null)
            {
                _stateService = new GameStateService();
                ServiceLocator.Register<IGameStateService>(_stateService);
                LoggingUtility.LogState("GameStateService registered with ServiceLocator");
            }

            SubscribeToStateEvents();
        }

        private void SubscribeToStateEvents()
        {
            if (_stateService == null) return;

            _stateService.OnEnterIntroVideo += HandleIntroVideo;
            _stateService.OnEnterClueSearch += HandleClueSearch;
            _stateService.OnEnterClueMatching += HandleClueMatching;
            _stateService.OnEnterRoleAssignment += HandleRoleAssignment;
            _stateService.OnEnterResults += HandleResults;

            _stateService.OnStateChanged += OnStateChanged;
            _stateService.OnStateTransitionComplete += OnStateTransitionComplete;

            LoggingUtility.LogState("Subscribed to state events");
        }

        private void UnsubscribeFromStateEvents()
        {
            if (_stateService == null) return;

            _stateService.OnEnterIntroVideo -= HandleIntroVideo;
            _stateService.OnEnterClueSearch -= HandleClueSearch;
            _stateService.OnEnterClueMatching -= HandleClueMatching;
            _stateService.OnEnterRoleAssignment -= HandleRoleAssignment;
            _stateService.OnEnterResults -= HandleResults;

            _stateService.OnStateChanged -= OnStateChanged;
            _stateService.OnStateTransitionComplete -= OnStateTransitionComplete;
        }

        #endregion

        #region State Handlers

        private void HandleIntroVideo()
        {
            LoggingUtility.LogState("Handling IntroVideo state entry");

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowVideoPlayer();
            }
        }

        private void HandleClueSearch()
        {
            LoggingUtility.LogState("Handling ClueSearch state entry");

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowClueSearch();
            }
        }

        private void HandleClueMatching()
        {
            LoggingUtility.LogState("Handling ClueMatching state entry");

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowClueMatching();
            }
        }

        private void HandleRoleAssignment()
        {
            LoggingUtility.LogState("Handling RoleAssignment state entry");

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RevealSuspects();
            }

            var roleService = ServiceLocator.Get<IRoleAssignmentService>();
            if (roleService != null && !roleService.IsInitialized)
            {
                if (GameManager.Instance?.CurrentLevel != null)
                {
                    var level = GameManager.Instance.CurrentLevel;
                    roleService.Initialize(level.Suspects, level.MaxDrugTestsPerLevel);
                    LoggingUtility.LogState("RoleAssignmentService initialized in HandleRoleAssignment");
                }
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowRoleAssignment();
            }
        }

        private void HandleResults()
        {
            LoggingUtility.LogState("Handling Results state entry");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RebuildVerdictRecordsFromService();
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowResults();
            }
        }

        private void OnStateChanged(GameState newState)
        {
            LoggingUtility.LogState($"State changed to: {newState}");
        }

        private void OnStateTransitionComplete(GameState oldState, GameState newState)
        {
            LoggingUtility.LogState($"State transition complete: {oldState} -> {newState}");
        }

        #endregion
    }
}