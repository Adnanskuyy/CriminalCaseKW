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
    public class SuspectDetailUI : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;

        private IRoleAssignmentService _roleService;
        private IClueMatchingService _matchingService;
        private SuspectData _currentSuspect;

        private Label _suspectNameLabel;
        private Label _drugTestCounterLabel;
        private Label _descriptionLabel;
        private Label _evidenceTextLabel;
        private VisualElement _evidenceClueIcons;
        private Label _drugTestResultLabel;
        private VisualElement _portraitImage;
        private Label _portraitPlaceholder;
        private Button _drugTestButton;
        private Button _verdictUserButton;
        private Button _verdictDealerButton;
        private Button _verdictNormalButton;
        private Button _closeButton;

        private bool _isBound;

        #region Unity Lifecycle

        private void OnEnable()
        {
            if (_document != null && _document.rootVisualElement != null)
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
                _roleService.OnRoleAssigned += OnRoleAssigned;
                _roleService.OnRoleChanged += OnRoleChanged;
                _roleService.OnDrugTestUsed += OnDrugTestUsed;
            }
        }

        private void UnsubscribeFromServices()
        {
            if (_roleService != null)
            {
                _roleService.OnRoleAssigned -= OnRoleAssigned;
                _roleService.OnRoleChanged -= OnRoleChanged;
                _roleService.OnDrugTestUsed -= OnDrugTestUsed;
            }
        }

        #endregion

        #region Public Methods

        public void Populate(SuspectData suspect)
        {
            if (!_isBound) BindUI();
            EnsureServices();

            _currentSuspect = suspect;
            UpdateUI();
        }

        #endregion

        #region UI Binding

        private void BindUI()
        {
            if (_document == null) return;
            if (_isBound) return;

            var root = _document.rootVisualElement;
            if (root == null) return;

            _suspectNameLabel = root.Q<Label>("suspect-name-label");
            _drugTestCounterLabel = root.Q<Label>("drug-test-counter-label");
            _descriptionLabel = root.Q<Label>("description-label");
            _evidenceTextLabel = root.Q<Label>("evidence-text-label");
            _evidenceClueIcons = root.Q<VisualElement>("evidence-clue-icons");
            _drugTestResultLabel = root.Q<Label>("drug-test-result-label");
            _portraitImage = root.Q<VisualElement>("portrait-image");
            _portraitPlaceholder = root.Q<Label>("portrait-placeholder");
            _drugTestButton = root.Q<Button>("drug-test-button");

            if (_drugTestButton != null)
                _drugTestButton.clicked += OnDrugTestClicked;

            _verdictUserButton = root.Q<Button>("verdict-user-button");
            if (_verdictUserButton != null)
                _verdictUserButton.clicked += () => OnVerdictClicked(SuspectRole.User);

            _verdictDealerButton = root.Q<Button>("verdict-dealer-button");
            if (_verdictDealerButton != null)
                _verdictDealerButton.clicked += () => OnVerdictClicked(SuspectRole.Dealer);

            _verdictNormalButton = root.Q<Button>("verdict-normal-button");
            if (_verdictNormalButton != null)
                _verdictNormalButton.clicked += () => OnVerdictClicked(SuspectRole.Normal);

            _closeButton = root.Q<Button>("detail-close-button");
            if (_closeButton != null)
                _closeButton.clicked += OnCloseClicked;

            _isBound = true;
        }

        private void UnbindUI()
        {
            if (_drugTestButton != null) _drugTestButton.clicked -= OnDrugTestClicked;
            if (_closeButton != null) _closeButton.clicked -= OnCloseClicked;
            _isBound = false;
        }

        #endregion

        #region UI Updates

        private void UpdateUI()
        {
            if (_currentSuspect == null) return;
            EnsureServices();

            UpdateSuspectInfo();
            UpdatePortrait();
            UpdateDrugTestSection();
            UpdateEvidenceSection();
            UpdateVerdictButtons();
            UpdateDrugTestCounter();
        }

        private void UpdateSuspectInfo()
        {
            if (_suspectNameLabel != null)
                _suspectNameLabel.text = _currentSuspect.SuspectName;

            if (_descriptionLabel != null)
                _descriptionLabel.text = _currentSuspect.Description;
        }

        private void UpdatePortrait()
        {
            if (_portraitImage != null)
            {
                if (_currentSuspect.Portrait != null)
                {
                    _portraitImage.style.backgroundImage = new StyleBackground(_currentSuspect.Portrait);
                    _portraitImage.style.display = DisplayStyle.Flex;
                }
                else
                {
                    _portraitImage.style.display = DisplayStyle.None;
                }
            }

            if (_portraitPlaceholder != null)
            {
                _portraitPlaceholder.style.display = _currentSuspect.Portrait != null
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
            }
        }

        private void UpdateDrugTestSection()
        {
            bool alreadyTested = _roleService != null && _roleService.HasDrugTestResult(_currentSuspect);
            bool hasTestsRemaining = _roleService != null && _roleService.DrugTestsRemaining > 0;

            if (_drugTestButton != null)
                _drugTestButton.SetEnabled(!alreadyTested && hasTestsRemaining);

            if (_drugTestResultLabel != null)
            {
                if (alreadyTested)
                {
                    var result = _roleService.GetDrugTestResult(_currentSuspect);
                    _drugTestResultLabel.text = $"Hasil: {result.ToDisplayName()}";
                    _drugTestResultLabel.RemoveFromClassList("positive");
                    _drugTestResultLabel.RemoveFromClassList("negative");
                    _drugTestResultLabel.AddToClassList(result == DrugTestResult.Positive ? "positive" : "negative");
                }
                else
                {
                    _drugTestResultLabel.text = string.Empty;
                    _drugTestResultLabel.RemoveFromClassList("positive");
                    _drugTestResultLabel.RemoveFromClassList("negative");
                }
            }
        }

        private void UpdateEvidenceSection()
        {
            if (_evidenceTextLabel == null) return;

            var matchedClueNames = new List<string>();
            var matchedClueIcons = new List<ClueData>();

            if (_matchingService != null && _matchingService.IsConfirmed)
            {
                var clues = _matchingService.GetCluesForSuspect(_currentSuspect);
                if (clues != null && clues.Count > 0)
                {
                    foreach (var clue in clues)
                    {
                        matchedClueNames.Add(clue.ClueName);
                        matchedClueIcons.Add(clue);
                    }
                }
            }

            if (matchedClueNames.Count > 0)
            {
                _evidenceTextLabel.text = string.Join("\n", matchedClueNames);
            }
            else
            {
                _evidenceTextLabel.text = _currentSuspect.EvidenceText;
            }

            UpdateEvidenceClueIcons(matchedClueIcons);
        }

        private void UpdateEvidenceClueIcons(List<ClueData> clues)
        {
            if (_evidenceClueIcons == null) return;
            _evidenceClueIcons.Clear();

            if (clues == null || clues.Count == 0) return;

            foreach (var clue in clues)
            {
                var icon = new VisualElement();
                icon.AddToClassList("evidence-clue-icon");

                if (clue.ClueIcon != null)
                {
                    var img = new VisualElement();
                    img.AddToClassList("evidence-clue-icon-image");
                    img.style.backgroundImage = new StyleBackground(clue.ClueIcon);
                    icon.Add(img);
                }

                _evidenceClueIcons.Add(icon);
            }
        }

        private void UpdateVerdictButtons()
        {
            if (_verdictUserButton == null || _verdictDealerButton == null || _verdictNormalButton == null)
                return;

            _verdictUserButton.text = SuspectRole.User.ToDisplayName();
            _verdictDealerButton.text = SuspectRole.Dealer.ToDisplayName();
            _verdictNormalButton.text = SuspectRole.Normal.ToDisplayName();

            var currentRole = _roleService?.GetAssignedRole(_currentSuspect);

            UpdateVerdictButtonStyle(_verdictUserButton, SuspectRole.User, currentRole);
            UpdateVerdictButtonStyle(_verdictDealerButton, SuspectRole.Dealer, currentRole);
            UpdateVerdictButtonStyle(_verdictNormalButton, SuspectRole.Normal, currentRole);
        }

        private void UpdateVerdictButtonStyle(Button button, SuspectRole buttonRole, SuspectRole? currentRole)
        {
            bool isSelected = currentRole.HasValue && currentRole.Value == buttonRole;

            if (isSelected)
                button.AddToClassList("selected");
            else
                button.RemoveFromClassList("selected");

            button.SetEnabled(true);
        }

        private void UpdateDrugTestCounter()
        {
            if (_drugTestCounterLabel == null || _roleService == null) return;
            _drugTestCounterLabel.text = $"Tes Narkoba Tersisa: {_roleService.DrugTestsRemaining}/{_roleService.MaxDrugTests}";
        }

        #endregion

        #region Event Handlers

        private void OnDrugTestClicked()
        {
            if (_roleService == null || _currentSuspect == null) return;
            if (!_roleService.UseDrugTest(_currentSuspect)) return;
            UpdateDrugTestSection();
            UpdateDrugTestCounter();
        }

        private void OnVerdictClicked(SuspectRole role)
        {
            if (_roleService == null || _currentSuspect == null) return;

            var currentRole = _roleService.GetAssignedRole(_currentSuspect);
            if (currentRole.HasValue && currentRole.Value == role) return;

            _roleService.AssignRole(_currentSuspect, role);
            UpdateVerdictButtons();
        }

        private void OnCloseClicked()
        {
            UIManager.Instance?.ShowCheckStatus();
        }

        private void OnRoleAssigned(SuspectData suspect, SuspectRole role)
        {
            if (suspect == _currentSuspect)
                UpdateVerdictButtons();
        }

        private void OnRoleChanged(SuspectData suspect, SuspectRole role)
        {
            if (suspect == _currentSuspect)
                UpdateVerdictButtons();
        }

        private void OnDrugTestUsed(SuspectData suspect, DrugTestResult result)
        {
            UpdateDrugTestCounter();
            if (suspect == _currentSuspect)
                UpdateDrugTestSection();
        }

        #endregion
    }
}