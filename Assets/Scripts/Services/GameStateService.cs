using System;
using CriminalCase2.Data;
using CriminalCase2.Services.Interfaces;
using CriminalCase2.Utils;

namespace CriminalCase2.Services
{
    /// <summary>
    /// Service implementation for game state management.
    /// Wraps GameStateMachine and provides service interface.
    /// </summary>
    public class GameStateService : IGameStateService
    {
        private readonly GameStateMachine _stateMachine;

        public GameStateService()
        {
            _stateMachine = new GameStateMachine();
            
            // Forward events from state machine
            _stateMachine.OnStateChanged += state => OnStateChanged?.Invoke(state);
            _stateMachine.OnStateTransitionComplete += (oldState, newState) => 
                OnStateTransitionComplete?.Invoke(oldState, newState);
            
            _stateMachine.OnEnterIntroVideo += () => OnEnterIntroVideo?.Invoke();
            _stateMachine.OnEnterClueSearch += () => OnEnterClueSearch?.Invoke();
            _stateMachine.OnEnterDeduction += () => OnEnterDeduction?.Invoke();
            _stateMachine.OnEnterResults += () => OnEnterResults?.Invoke();
            
            _stateMachine.OnExitIntroVideo += () => OnExitIntroVideo?.Invoke();
            _stateMachine.OnExitClueSearch += () => OnExitClueSearch?.Invoke();
            _stateMachine.OnExitDeduction += () => OnExitDeduction?.Invoke();
            _stateMachine.OnExitResults += () => OnExitResults?.Invoke();
        }

        #region IGameStateService Implementation

        public event Action<GameState> OnStateChanged;
        public event Action<GameState, GameState> OnStateTransitionComplete;

        public GameState CurrentState => _stateMachine.CurrentState;
        public GameState? PreviousState => _stateMachine.PreviousState;
        public bool IsTransitioning => _stateMachine.IsTransitioning;

        public void Initialize(GameState initialState)
        {
            _stateMachine.Initialize(initialState);
            LoggingUtility.LogState($"Service initialized with state: {initialState}");
        }

        public void TransitionTo(GameState newState)
        {
            _stateMachine.TransitionTo(newState);
        }

        public bool IsInState(GameState state)
        {
            return _stateMachine.IsInState(state);
        }

        public bool CanTransitionTo(GameState state)
        {
            return _stateMachine.CanTransitionTo(state);
        }

        #endregion

        #region State-Specific Events

        public event Action OnEnterIntroVideo;
        public event Action OnEnterClueSearch;
        public event Action OnEnterDeduction;
        public event Action OnEnterResults;

        public event Action OnExitIntroVideo;
        public event Action OnExitClueSearch;
        public event Action OnExitDeduction;
        public event Action OnExitResults;

        #endregion
    }
}
