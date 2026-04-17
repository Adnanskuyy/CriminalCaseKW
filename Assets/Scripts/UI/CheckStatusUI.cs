using UnityEngine;
using UnityEngine.UIElements;
using CriminalCase2.Data;
using CriminalCase2.Managers;
using CriminalCase2.Services;
using CriminalCase2.Services.Interfaces;
using CriminalCase2.Utils;
using System.Collections.Generic;

namespace CriminalCase2.UI
{
    public class CheckStatusUI : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;

        private IRoleAssignmentService _roleService;
        private IClueMatchingService _matchingService;

        private VisualElement _container;
        private VisualElement _emptyState;
        private Button _closeButton;
        private Button _checkResultButton;
        private Label _drugTestGlobalCounter;
        private bool _isBound;

        private SuspectData[] _suspects;

        #region Unity Lifecycle

        private void OnEnable()
        {
            BindUI();
            SubscribeToServices();
        }

        private void OnDisable()
        {
            UnsubscribeFromServices();
            UnbindUI();
        }

        #endregion

        #region Service Integration

        private void EnsureServices()
        {
            if (_roleService == null)
                _roleService = ServiceLocator.Get<IRoleAssignmentService>();

            if (_matchingService == null)
                _matchingService = ServiceLocator.Get<IClueMatchingService>();
        }

        private void SubscribeToServices()
        {
            EnsureServices();
            if (_roleService != null)
            {
                _roleService.OnRoleAssigned += OnRoleChanged;
                _roleService.OnRoleChanged += OnRoleChanged;
                _roleService.OnDrugTestUsed += OnDrugTestUsed;
                _roleService.OnAllRolesAssigned += OnAllRolesAssigned;
            }
        }

        private void UnsubscribeFromServices()
        {
            if (_roleService != null)
            {
                _roleService.OnRoleAssigned -= OnRoleChanged;
                _roleService.OnRoleChanged -= OnRoleChanged;
                _roleService.OnDrugTestUsed -= OnDrugTestUsed;
                _roleService.OnAllRolesAssigned -= OnAllRolesAssigned;
            }
        }

        #endregion

        #region UI Binding

        private void BindUI()
        {
            if (_document == null || _isBound) return;

            var root = _document.rootVisualElement;
            if (root == null) return;

            _container = root.Q<VisualElement>("check-status-container");
            _emptyState = root.Q<VisualElement>("check-status-empty");
            _closeButton = root.Q<Button>("check-status-close-button");
            _checkResultButton = root.Q<Button>("check-result-button");
            _drugTestGlobalCounter = root.Q<Label>("drug-test-global-counter");

            if (_closeButton != null)
                _closeButton.clicked += OnCloseClicked;

            if (_checkResultButton != null)
                _checkResultButton.clicked += OnCheckResultClicked;

            _isBound = true;
        }

        private void UnbindUI()
        {
            if (_closeButton != null)
            {
                _closeButton.clicked -= OnCloseClicked;
                _closeButton = null;
            }

            if (_checkResultButton != null)
            {
                _checkResultButton.clicked -= OnCheckResultClicked;
                _checkResultButton = null;
            }

            _container = null;
            _drugTestGlobalCounter = null;
            _isBound = false;
        }

        #endregion

        #region Public Methods

        public void Populate(IReadOnlyList<VerdictRecord> records)
        {
            if (!_isBound) BindUI();
            EnsureServices();
            RebuildEntries();
            UpdateCheckResultButton();
            UpdateDrugTestCounter();
        }

        public void Refresh()
        {
            if (!_isBound) BindUI();
            EnsureServices();
            RebuildEntries();
            UpdateCheckResultButton();
            UpdateDrugTestCounter();
        }

        #endregion

        #region UI Updates

        private void RebuildEntries()
        {
            if (_container == null) return;
            _container.Clear();

            if (_roleService == null || _roleService.Suspects == null) return;

            _suspects = new SuspectData[_roleService.Suspects.Count];
            for (int i = 0; i < _roleService.Suspects.Count; i++)
                _suspects[i] = _roleService.Suspects[i];

            bool hasAnyData = _roleService.AssignedCount > 0 || _roleService.DrugTestsUsed > 0;

            if (_container != null)
                _container.style.display = _suspects.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            if (_emptyState != null)
                _emptyState.style.display = _suspects.Length == 0 ? DisplayStyle.Flex : DisplayStyle.None;

            for (int i = 0; i < _suspects.Length; i++)
            {
                var suspect = _suspects[i];
                var entry = CreateStatusEntry(suspect, i);
                _container.Add(entry);
            }
        }

        private VisualElement CreateStatusEntry(SuspectData suspect, int index)
        {
            var entry = new VisualElement();
            entry.AddToClassList("check-status-entry");
            entry.userData = suspect;

            var leftSection = new VisualElement();
            leftSection.AddToClassList("check-status-entry-left");

            var portraitContainer = new VisualElement();
            portraitContainer.AddToClassList("check-status-portrait");

            if (suspect.Portrait != null)
            {
                var portraitImg = new VisualElement();
                portraitImg.AddToClassList("check-status-portrait-image");
                portraitImg.style.backgroundImage = new StyleBackground(suspect.Portrait);
                portraitContainer.Add(portraitImg);
            }
            else
            {
                var initial = new Label(suspect.SuspectName.Length > 0 ? suspect.SuspectName.Substring(0, 1).ToUpper() : "?");
                initial.AddToClassList("check-status-portrait-initial");
                portraitContainer.Add(initial);
            }

            leftSection.Add(portraitContainer);

            var infoSection = new VisualElement();
            infoSection.AddToClassList("check-status-entry-info");

            var nameLabel = new Label(suspect.SuspectName);
            nameLabel.AddToClassList("check-status-name");
            infoSection.Add(nameLabel);

            if (_roleService.HasDrugTestResult(suspect))
            {
                var drugResult = _roleService.GetDrugTestResult(suspect);
                var drugLabel = new Label($"Tes Narkoba: {drugResult.ToDisplayName()}");
                drugLabel.AddToClassList("check-status-drug-result");
                drugLabel.AddToClassList(drugResult == DrugTestResult.Positive ? "positive" : "negative");
                infoSection.Add(drugLabel);
            }

            leftSection.Add(infoSection);
            entry.Add(leftSection);

            var assignedRole = _roleService.GetAssignedRole(suspect);
            var verdictLabel = new Label();
            verdictLabel.AddToClassList("check-status-verdict");

            if (assignedRole.HasValue)
            {
                verdictLabel.text = assignedRole.Value.ToDisplayName();
            }
            else
            {
                verdictLabel.text = "Belum dipilih";
                verdictLabel.AddToClassList("unassigned");
            }

            entry.Add(verdictLabel);

            entry.RegisterCallback<PointerDownEvent>(evt =>
            {
                UIManager.Instance?.ShowSuspectDetail(suspect);
            });

            return entry;
        }

        private void UpdateCheckResultButton()
        {
            if (_checkResultButton == null || _roleService == null) return;

            bool allJudged = _roleService.AllRolesAssigned;
            _checkResultButton.SetEnabled(allJudged);

            if (!allJudged)
            {
                int remaining = _roleService.TotalSuspects - _roleService.AssignedCount;
                _checkResultButton.text = $"Kirim Vonis Akhir ({remaining} tersisa)";
            }
            else
            {
                _checkResultButton.text = "Kirim Vonis Akhir";
            }
        }

        private void UpdateDrugTestCounter()
        {
            if (_drugTestGlobalCounter == null || _roleService == null) return;
            _drugTestGlobalCounter.text = $"Tes Narkoba Tersisa: {_roleService.DrugTestsRemaining}/{_roleService.MaxDrugTests}";
        }

        #endregion

        #region Event Handlers

        private void OnCloseClicked()
        {
            UIManager.Instance?.HideCheckStatus();
        }

        private void OnCheckResultClicked()
        {
            if (_roleService != null && _roleService.AllRolesAssigned)
            {
                GameManager.Instance?.SetState(GameState.Results);
            }
        }

        private void OnRoleChanged(SuspectData suspect, SuspectRole role)
        {
            Refresh();
        }

        private void OnDrugTestUsed(SuspectData suspect, DrugTestResult result)
        {
            UpdateDrugTestCounter();
            Refresh();
        }

        private void OnAllRolesAssigned()
        {
            UpdateCheckResultButton();
        }

        #endregion
    }
}