using UnityEngine;
using UnityEngine.UIElements;
using CriminalCase2.Data;
using CriminalCase2.Managers;

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
        private bool _hasVerdict;
        private SuspectRole _selectedVerdict;

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

            _suspectNameLabel = root.Q<Label>("suspect-name-label");
            _descriptionLabel = root.Q<Label>("description-label");
            _evidenceTextLabel = root.Q<Label>("evidence-text-label");
            _drugTestResultLabel = root.Q<Label>("drug-test-result-label");

            _drugTestButton = root.Q<Button>("drug-test-button");
            if (_drugTestButton != null)
            {
                _drugTestButton.clicked += OnDrugTestClicked;
            }

            _verdictUserButton = root.Q<Button>("verdict-user-button");
            if (_verdictUserButton != null)
            {
                _verdictUserButton.clicked += () => OnVerdictClicked(SuspectRole.User);
            }

            _verdictDealerButton = root.Q<Button>("verdict-dealer-button");
            if (_verdictDealerButton != null)
            {
                _verdictDealerButton.clicked += () => OnVerdictClicked(SuspectRole.Dealer);
            }

            _verdictNormalButton = root.Q<Button>("verdict-normal-button");
            if (_verdictNormalButton != null)
            {
                _verdictNormalButton.clicked += () => OnVerdictClicked(SuspectRole.Normal);
            }

            _closeButton = root.Q<Button>("detail-close-button");
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
            if (_drugTestResultLabel != null) _drugTestResultLabel.text = string.Empty;

            if (_drugTestButton != null)
            {
                _drugTestButton.SetEnabled(LevelManager.Instance != null && LevelManager.Instance.DrugTestsRemaining > 0);
            }

            _hasVerdict = LevelManager.Instance != null && LevelManager.Instance.IsSuspectJudged(_currentSuspect);
            if (_hasVerdict)
            {
                _selectedVerdict = LevelManager.Instance.GetSuspectVerdict(_currentSuspect);
            }
            UpdateVerdictButtons();
        }

        private void OnDrugTestClicked()
        {
            if (LevelManager.Instance == null || _currentSuspect == null) return;

            if (LevelManager.Instance.UseDrugTest())
            {
                var result = _currentSuspect.DrugTestResult;
                if (_drugTestResultLabel != null)
                {
                    _drugTestResultLabel.text = result.ToDisplayName();
                }
            }
        }

        private void OnVerdictClicked(SuspectRole role)
        {
            if (LevelManager.Instance == null || _currentSuspect == null) return;

            LevelManager.Instance.RecordJudgedSuspect(_currentSuspect, role);
            UIManager.Instance?.HideAllPanels();
            UIManager.Instance?.ShowStatusHUD();
            UIManager.Instance?.UpdateStatusHUD();
        }

        private void OnCloseClicked()
        {
            UIManager.Instance?.HideAllPanels();
        }

        private void UpdateVerdictButtons()
        {
            if (_verdictUserButton == null || _verdictDealerButton == null || _verdictNormalButton == null)
                return;

            if (_hasVerdict)
            {
                _verdictUserButton.SetEnabled(false);
                _verdictDealerButton.SetEnabled(false);
                _verdictNormalButton.SetEnabled(false);

                var selectedButton = _selectedVerdict switch
                {
                    SuspectRole.User => _verdictUserButton,
                    SuspectRole.Dealer => _verdictDealerButton,
                    SuspectRole.Normal => _verdictNormalButton,
                    _ => null
                };

                if (selectedButton != null)
                {
                    selectedButton.text = $"{selectedButton.text} [DIPILIH]";
                }
            }
            else
            {
                _verdictUserButton.SetEnabled(true);
                _verdictDealerButton.SetEnabled(true);
                _verdictNormalButton.SetEnabled(true);

                _verdictUserButton.text = SuspectRole.User.ToDisplayName();
                _verdictDealerButton.text = SuspectRole.Dealer.ToDisplayName();
                _verdictNormalButton.text = SuspectRole.Normal.ToDisplayName();
            }
        }
    }
}
