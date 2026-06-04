using UnityEngine;
using CriminalCase2.Data;
using CriminalCase2.Services;
using CriminalCase2.UI;

namespace CriminalCase2.Managers
{
    public class GameStateController : MonoBehaviour
    {
        private void OnEnable()
        {
            var gs = GameServices.GameState;
            if (gs != null)
            {
                gs.StateChanged += OnStateChanged;
            }
        }

        private void OnDisable()
        {
            var gs = GameServices.GameState;
            if (gs != null)
            {
                gs.StateChanged -= OnStateChanged;
            }
        }

        private void OnStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.IntroVideo:
                    HandleIntroVideo();
                    break;
                case GameState.Tutorial:
                    HandleTutorial();
                    break;
                case GameState.Investigation:
                    HandleInvestigation();
                    break;
                case GameState.Verdict:
                    HandleVerdict();
                    break;
                case GameState.Results:
                    HandleResults();
                    break;
            }
        }

        private void HandleIntroVideo()
        {
            GameServices.UI?.ShowVideoPlayer();
        }

        private void HandleTutorial()
        {
            GameServices.GameState?.SetState(GameState.Investigation);
        }

        private void HandleInvestigation()
        {
            GameServices.UI?.ShowStatusHUD();
        }

        private void HandleVerdict()
        {
        }

        private void HandleResults()
        {
            GameServices.UI?.ShowResults();
        }
    }
}
