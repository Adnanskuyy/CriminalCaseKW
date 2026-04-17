using UnityEngine;
using UnityEngine.UIElements;
using CriminalCase2.Data;
using CriminalCase2.Managers;
using CriminalCase2.Services;
using CriminalCase2.Services.Interfaces;
using CriminalCase2.Utils;

namespace CriminalCase2.UI
{
    public class StatusHUD : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;

        private IRoleAssignmentService _roleService;

        private Button _hudButton;
        private Label _drugCounterLabel;
        private bool _isBound;

        #region Unity Lifecycle

        private void OnEnable()
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState != GameState.RoleAssignment &&
                GameManager.Instance.CurrentState != GameState.Results)
            {
                if (_document != null && _document.rootVisualElement != null)
                {
                    var panelElement = _document.rootVisualElement.Q<VisualElement>(className: "panel");
                    if (panelElement != null)
                        panelElement.style.display = DisplayStyle.None;
                    else
                        _document.rootVisualElement.style.display = DisplayStyle.None;
                }
                return;
            }

            if (_document != null && _document.rootVisualElement != null)
            {
                var panelElement = _document.rootVisualElement.Q<VisualElement>(className: "panel");
                if (panelElement != null)
                    panelElement.style.display = DisplayStyle.Flex;
                else
                    _document.rootVisualElement.style.display = DisplayStyle.Flex;

                BindUI();
                UpdateButtonText();
            }

            SubscribeToService();
        }

        private void OnDisable()
        {
            UnsubscribeFromService();
            UnbindUI();
        }

        #endregion

        #region Service Integration

        private void EnsureService()
        {
            if (_roleService == null)
                _roleService = ServiceLocator.Get<IRoleAssignmentService>();
        }

        private void SubscribeToService()
        {
            EnsureService();
            if (_roleService != null)
            {
                _roleService.OnRoleAssigned += OnRoleChanged;
                _roleService.OnRoleChanged += OnRoleChanged;
                _roleService.OnDrugTestUsed += OnDrugTestUsed;
            }
        }

        private void UnsubscribeFromService()
        {
            if (_roleService != null)
            {
                _roleService.OnRoleAssigned -= OnRoleChanged;
                _roleService.OnRoleChanged -= OnRoleChanged;
                _roleService.OnDrugTestUsed -= OnDrugTestUsed;
            }
        }

        #endregion

        #region Public Methods

        public void Initialize()
        {
            if (!_isBound) BindUI();

            if (_document != null && _document.rootVisualElement != null)
            {
                var panelElement = _document.rootVisualElement.Q<VisualElement>(className: "panel");
                if (panelElement != null)
                    panelElement.style.display = DisplayStyle.Flex;
                else
                    _document.rootVisualElement.style.display = DisplayStyle.Flex;
            }

            UpdateButtonText();
            UpdateDrugCounter();
        }

        #endregion

        #region UI Binding

        private void BindUI()
        {
            if (_document == null || _isBound) return;

            var root = _document.rootVisualElement;
            if (root == null) return;

            _hudButton = root.Q<Button>("status-hud-button");
            if (_hudButton != null)
                _hudButton.clicked += OnHudButtonClicked;

            var hudPanel = root.Q<VisualElement>("status-hud-panel");
            if (hudPanel != null)
            {
                _drugCounterLabel = hudPanel.Q<Label>("hud-drug-counter");
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
            _drugCounterLabel = null;
            _isBound = false;
        }

        #endregion

        #region UI Updates

        public void UpdateButtonText()
        {
            if (_hudButton == null) return;
            EnsureService();

            if (_roleService == null)
            {
                if (LevelManager.Instance != null)
                {
                    int judged = LevelManager.Instance.JudgedCount;
                    int totalSuspects = LevelManager.Instance.TotalSuspects;
                    _hudButton.text = judged >= totalSuspects
                        ? $"Lihat Hasil ({judged}/{totalSuspects})"
                        : $"Cek Status ({judged}/{totalSuspects})";
                }
                return;
            }

            int assignedCount = _roleService.AssignedCount;
            int totalSuspectCount = _roleService.TotalSuspects;

            if (assignedCount >= totalSuspectCount)
                _hudButton.text = $"Lihat Hasil ({assignedCount}/{totalSuspectCount})";
            else if (assignedCount > 0)
                _hudButton.text = $"Cek Status ({assignedCount}/{totalSuspectCount})";
            else
                _hudButton.text = $"Cek Status (0/{totalSuspectCount})";
        }

        public void UpdateDrugCounter()
        {
            if (_drugCounterLabel == null) return;
            EnsureService();

            if (_roleService != null)
            {
                _drugCounterLabel.text = $"Tes Narkoba: {_roleService.DrugTestsRemaining}/{_roleService.MaxDrugTests}";
                _drugCounterLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                _drugCounterLabel.style.display = DisplayStyle.None;
            }
        }

        #endregion

        #region Event Handlers

        private void OnHudButtonClicked()
        {
            UIManager.Instance?.ShowCheckStatus();
        }

        private void OnRoleChanged(SuspectData suspect, SuspectRole role)
        {
            UpdateButtonText();
        }

        private void OnDrugTestUsed(SuspectData suspect, DrugTestResult result)
        {
            UpdateDrugCounter();
            UpdateButtonText();
        }

        #endregion
    }
}