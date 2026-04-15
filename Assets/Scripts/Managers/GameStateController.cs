using UnityEngine;
using CriminalCase2.Data;
using CriminalCase2.UI;

namespace CriminalCase2.Managers
{
    public class GameStateController : MonoBehaviour
    {
        private GameState? _lastHandledState = null;

        private void Update()
        {
            if (GameManager.Instance == null) return;

            GameState currentState = GameManager.Instance.CurrentState;

            if (_lastHandledState.HasValue && currentState == _lastHandledState.Value) return;
            _lastHandledState = currentState;

            switch (currentState)
            {
                case GameState.IntroVideo:
                    HandleIntroVideo();
                    break;
                case GameState.ClueSearch:
                    HandleClueSearch();
                    break;
                case GameState.Deduction:
                    HandleDeduction();
                    break;
                case GameState.Results:
                    HandleResults();
                    break;
            }
        }

        private void HandleIntroVideo()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowVideoPlayer();
            }
        }

        private void HandleClueSearch()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowClueSearch();
            }
        }

        private void HandleDeduction()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowStatusHUD();
            }
        }

        private void HandleResults()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowResults();
            }
        }
    }
}
