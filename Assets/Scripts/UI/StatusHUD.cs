using UnityEngine;
using UnityEngine.UIElements;
using CriminalCase2.Data;
using CriminalCase2.Managers;

namespace CriminalCase2.UI
{
    public class StatusHUD : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;

        private Button _hudButton;
        private bool _isBound;

        public void Initialize()
        {
            if (!_isBound) BindUI();
            UpdateButtonText();
        }

        private void OnEnable()
        {
            // Only show StatusHUD during RoleAssignment or Results phase
            if (GameManager.Instance != null && 
                GameManager.Instance.CurrentState != GameState.RoleAssignment &&
                GameManager.Instance.CurrentState != GameState.Results)
            {
                // Hide the panel if we're not in the correct state
                if (_document != null && _document.rootVisualElement != null)
                {
                    var panelElement = _document.rootVisualElement.Q<VisualElement>(className: "panel");
                    if (panelElement != null)
                    {
                        panelElement.style.display = DisplayStyle.None;
                    }
                    else
                    {
                        _document.rootVisualElement.style.display = DisplayStyle.None;
                    }
                }
                return;
            }

            if (_document != null && _document.rootVisualElement != null)
            {
                BindUI();
                UpdateButtonText();
            }
        }

        private void OnDisable()
        {
            UnbindUI();
        }

        private void BindUI()
        {
            if (_document == null || _isBound) return;

            var root = _document.rootVisualElement;
            if (root == null) return;

            _hudButton = root.Q<Button>("status-hud-button");
            if (_hudButton != null)
            {
                _hudButton.clicked += OnHudButtonClicked;
            }

            _isBound = true;
            UpdateButtonText();
        }

        private void UnbindUI()
        {
            if (_hudButton != null)
            {
                _hudButton.clicked -= OnHudButtonClicked;
                _hudButton = null;
            }
            _isBound = false;
        }

        public void UpdateButtonText()
        {
            if (_hudButton == null || LevelManager.Instance == null) return;

            var judged = LevelManager.Instance.JudgedCount;
            var total = LevelManager.Instance.TotalSuspects;

            if (judged >= total)
            {
                _hudButton.text = $"Lihat Hasil ({judged}/{total})";
            }
            else if (judged > 0)
            {
                _hudButton.text = $"Cek Status ({judged}/{total})";
            }
            else
            {
                _hudButton.text = $"Cek Status (0/{total})";
            }
        }

        private void OnHudButtonClicked()
        {
            UIManager.Instance?.ShowCheckStatus();
        }
    }
}
