using UnityEngine;
using UnityEngine.UIElements;
using CriminalCase2.Services;

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

            _hudButton = root.Q<Button>(UIConstants.StatusHud.Button);
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
            var levels = GameServices.Levels;
            if (_hudButton == null || levels == null) return;

            var judged = levels.JudgedCount;
            var total = levels.TotalSuspects;

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
            GameServices.UI?.ShowCheckStatus();
        }
    }
}
