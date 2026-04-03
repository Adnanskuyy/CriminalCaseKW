using UnityEngine;
using CriminalCase2.Data;
using CriminalCase2.UI;

namespace CriminalCase2.Managers
{
    public class GameStateController : MonoBehaviour
    {
        private void Update()
        {
            if (GameManager.Instance == null) return;

            switch (GameManager.Instance.CurrentState)
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
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowVideoPlayer();
                GameManager.Instance.SetState(GameState.Tutorial);
            }
        }

        private void HandleTutorial()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowTutorial();
            }
        }

        private void HandleInvestigation()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideAllPanels();
            }
        }

        private void HandleVerdict()
        {
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
