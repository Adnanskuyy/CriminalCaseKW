using UnityEngine;
using UnityEngine.UIElements;
using CriminalCase2.Data;
using CriminalCase2.Services;

namespace CriminalCase2.UI
{
    public class SuspectDetailUI : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;

        private SuspectData _currentSuspect;

        private Label _suspectNameLabel;
        private Label _descriptionLabel;
        private Label _evidenceTextLabel;
        private Label _drugTestResultLabel;
        private Button _drugTestButton;
        private Button _verdictUserButton;
        private Button _verdictDealerButton;
        private Button _verdictNormalButton;
        private Button _closeButton;

        private bool _isBound;

        public void Populate(SuspectData suspect)
        {
            if (!_isBound) BindUI();

            _currentSuspect = suspect;
            UpdateUI();
        }

        private void OnEnable()
        {
            if (_document != null && _document.rootVisualElement != null)
            {
                BindUI();
            }
        }

        private void OnDisable()
        {
            UnbindUI();
        }

        private void BindUI()
        {
            if (_document == null) return;
            if (_isBound) return;

            var root = _document.rootVisualElement;
            if (root == null) return;

            _suspectNameLabel = root.Q<Label>(UIConstants.SuspectDetail.SuspectNameLabel);
            _descriptionLabel = root.Q<Label>(UIConstants.SuspectDetail.DescriptionLabel);
            _evidenceTextLabel = root.Q<Label>(UIConstants.SuspectDetail.EvidenceTextLabel);
            _drugTestResultLabel = root.Q<Label>(UIConstants.SuspectDetail.DrugTestResultLabel);

            _drugTestButton = root.Q<Button>(UIConstants.SuspectDetail.DrugTestButton);
            if (_drugTestButton != null)
            {
                _drugTestButton.clicked += OnDrugTestClicked;
            }

            _verdictUserButton = root.Q<Button>(UIConstants.SuspectDetail.VerdictUserButton);
            if (_verdictUserButton != null)
            {
                _verdictUserButton.clicked += () => OnVerdictClicked(SuspectRole.User);
            }

            _verdictDealerButton = root.Q<Button>(UIConstants.SuspectDetail.VerdictDealerButton);
            if (_verdictDealerButton != null)
            {
                _verdictDealerButton.clicked += () => OnVerdictClicked(SuspectRole.Dealer);
            }

            _verdictNormalButton = root.Q<Button>(UIConstants.SuspectDetail.VerdictNormalButton);
            if (_verdictNormalButton != null)
            {
                _verdictNormalButton.clicked += () => OnVerdictClicked(SuspectRole.Normal);
            }

            _closeButton = root.Q<Button>(UIConstants.SuspectDetail.CloseButton);
            if (_closeButton != null)
            {
                _closeButton.clicked += OnCloseClicked;
            }

            _isBound = true;
            UpdateUI();
        }

        private void UnbindUI()
        {
            if (_drugTestButton != null) _drugTestButton.clicked -= OnDrugTestClicked;
            if (_closeButton != null) _closeButton.clicked -= OnCloseClicked;
            _isBound = false;
        }

        private void UpdateUI()
        {
            if (_currentSuspect == null) return;

            if (_suspectNameLabel != null) _suspectNameLabel.text = _currentSuspect.SuspectName;
            if (_descriptionLabel != null) _descriptionLabel.text = _currentSuspect.Description;
            if (_evidenceTextLabel != null) _evidenceTextLabel.text = _currentSuspect.EvidenceText;
            if (_drugTestResultLabel != null)
            {
                var levels = GameServices.Levels;
                if (levels != null && levels.HasDrugTestResult(_currentSuspect))
                {
                    _drugTestResultLabel.text = levels.GetDrugTestResult(_currentSuspect).ToDisplayName();
                }
                else
                {
                    _drugTestResultLabel.text = string.Empty;
                }
            }

            if (_drugTestButton != null)
            {
                var levels = GameServices.Levels;
                bool alreadyTested = levels != null && levels.HasDrugTestResult(_currentSuspect);
                bool hasTestsRemaining = levels != null && levels.DrugTestsRemaining > 0;
                _drugTestButton.SetEnabled(!alreadyTested && hasTestsRemaining);
            }

            UpdateVerdictButtons();
        }

        private void OnDrugTestClicked()
        {
            var levels = GameServices.Levels;
            if (levels == null || _currentSuspect == null) return;

            if (levels.UseDrugTest())
            {
                var result = _currentSuspect.DrugTestResult;
                levels.RecordDrugTest(_currentSuspect, result);
                if (_drugTestResultLabel != null)
                {
                    _drugTestResultLabel.text = result.ToDisplayName();
                }
                if (_drugTestButton != null)
                {
                    _drugTestButton.SetEnabled(false);
                }
            }
        }

        private void OnVerdictClicked(SuspectRole role)
        {
            var levels = GameServices.Levels;
            if (levels == null || _currentSuspect == null) return;

            levels.RecordJudgedSuspect(_currentSuspect, role);
            GameServices.UI?.HideAllPanels();
            GameServices.UI?.ShowStatusHUD();
            GameServices.UI?.UpdateStatusHUD();
        }

        private void OnCloseClicked()
        {
            GameServices.UI?.HideAllPanels();
        }

        private void UpdateVerdictButtons()
        {
            if (_verdictUserButton == null || _verdictDealerButton == null || _verdictNormalButton == null)
                return;

            _verdictUserButton.text = SuspectRole.User.ToDisplayName();
            _verdictDealerButton.text = SuspectRole.Dealer.ToDisplayName();
            _verdictNormalButton.text = SuspectRole.Normal.ToDisplayName();

            _verdictUserButton.SetEnabled(true);
            _verdictDealerButton.SetEnabled(true);
            _verdictNormalButton.SetEnabled(true);
        }
    }
}
