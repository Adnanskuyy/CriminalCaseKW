using System;
using CriminalCase2.Data;
using CriminalCase2.Utils;

namespace CriminalCase2.Utils
{
    /// <summary>
    /// Pure C# state machine for game state management.
    /// Replaces polling-based state checking with event-driven architecture.
    /// </summary>
    public class GameStateMachine
    {
        private GameState _currentState;
        private GameState? _previousState;
        private bool _isTransitioning;

        #region Events

        public event Action<GameState> OnStateChanged;
        public event Action<GameState, GameState> OnStateTransitionComplete;

        // Specific state events
        public event Action OnEnterIntroVideo;
        public event Action OnEnterClueSearch;
        public event Action OnEnterDeduction;
        public event Action OnEnterResults;

        public event Action OnExitIntroVideo;
        public event Action OnExitClueSearch;
        public event Action OnExitDeduction;
        public event Action OnExitResults;

        #endregion

        #region Properties

        public GameState CurrentState => _currentState;
        public GameState? PreviousState => _previousState;
        public bool IsTransitioning => _isTransitioning;

        #endregion

        /// <summary>
        /// Initialize the state machine with a starting state.
        /// </summary>
        public void Initialize(GameState initialState)
        {
            if (_isTransitioning)
            {
                LoggingUtility.Warning("StateMachine", "Cannot initialize while transitioning");
                return;
            }

            _currentState = initialState;
            _previousState = null;
            _isTransitioning = false;

            LoggingUtility.LogState($"Initialized with state: {initialState}");
            
            EnterState(initialState);
            OnStateChanged?.Invoke(initialState);
        }

        /// <summary>
        /// Transition to a new state.
        /// </summary>
        public void TransitionTo(GameState newState)
        {
            if (_isTransitioning)
            {
                LoggingUtility.Warning("StateMachine", $"Cannot transition to {newState} - already transitioning");
                return;
            }

            if (_currentState == newState)
            {
                LoggingUtility.LogDebug("StateMachine", $"Already in state {newState}, skipping transition");
                return;
            }

            _isTransitioning = true;
            var oldState = _currentState;

            LoggingUtility.LogState($"Transitioning: {oldState} -> {newState}");

            // Exit current state
            ExitState(oldState);

            // Update state
            _previousState = oldState;
            _currentState = newState;

            // Enter new state
            EnterState(newState);

            // Notify listeners
            OnStateChanged?.Invoke(newState);
            OnStateTransitionComplete?.Invoke(oldState, newState);

            _isTransitioning = false;

            LoggingUtility.LogState($"Transition complete: {oldState} -> {newState}");
        }

        /// <summary>
        /// Check if currently in a specific state.
        /// </summary>
        public bool IsInState(GameState state)
        {
            return _currentState == state && !_isTransitioning;
        }

        /// <summary>
        /// Check if can transition to a specific state.
        /// </summary>
        public bool CanTransitionTo(GameState state)
        {
            return !_isTransitioning && _currentState != state;
        }

        private void EnterState(GameState state)
        {
            LoggingUtility.LogDebug("StateMachine", $"Entering state: {state}");

            switch (state)
            {
                case GameState.IntroVideo:
                    OnEnterIntroVideo?.Invoke();
                    break;
                case GameState.ClueSearch:
                    OnEnterClueSearch?.Invoke();
                    break;
                case GameState.Deduction:
                    OnEnterDeduction?.Invoke();
                    break;
                case GameState.Results:
                    OnEnterResults?.Invoke();
                    break;
            }
        }

        private void ExitState(GameState state)
        {
            LoggingUtility.LogDebug("StateMachine", $"Exiting state: {state}");

            switch (state)
            {
                case GameState.IntroVideo:
                    OnExitIntroVideo?.Invoke();
                    break;
                case GameState.ClueSearch:
                    OnExitClueSearch?.Invoke();
                    break;
                case GameState.Deduction:
                    OnExitDeduction?.Invoke();
                    break;
                case GameState.Results:
                    OnExitResults?.Invoke();
                    break;
            }
        }
    }
}
