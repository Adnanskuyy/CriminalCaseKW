using System;
using CriminalCase2.Data;

namespace CriminalCase2.Services.Interfaces
{
    /// <summary>
    /// Service interface for game state management.
    /// Provides access to the GameStateMachine and current state.
    /// </summary>
    public interface IGameStateService
    {
        /// <summary>
        /// Event fired when game state changes.
        /// </summary>
        event Action<GameState> OnStateChanged;

        /// <summary>
        /// Event fired when a state transition completes.
        /// Parameters: (oldState, newState)
        /// </summary>
        event Action<GameState, GameState> OnStateTransitionComplete;

        /// <summary>
        /// Current game state.
        /// </summary>
        GameState CurrentState { get; }

        /// <summary>
        /// Previous game state (null if no previous state).
        /// </summary>
        GameState? PreviousState { get; }

        /// <summary>
        /// Whether a state transition is in progress.
        /// </summary>
        bool IsTransitioning { get; }

        /// <summary>
        /// Initialize the service with a starting state.
        /// </summary>
        void Initialize(GameState initialState);

        /// <summary>
        /// Transition to a new state.
        /// </summary>
        void TransitionTo(GameState newState);

        /// <summary>
        /// Check if currently in a specific state.
        /// </summary>
        bool IsInState(GameState state);

        /// <summary>
        /// Check if can transition to a specific state.
        /// </summary>
        bool CanTransitionTo(GameState state);

        #region State-Specific Events

        event Action OnEnterIntroVideo;
        event Action OnEnterClueSearch;
        event Action OnEnterClueMatching;
        event Action OnEnterRoleAssignment;
        event Action OnEnterResults;

        event Action OnExitIntroVideo;
        event Action OnExitClueSearch;
        event Action OnExitClueMatching;
        event Action OnExitRoleAssignment;
        event Action OnExitResults;

        #endregion
    }
}
