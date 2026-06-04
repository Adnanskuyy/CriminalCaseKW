#nullable enable
using UnityEngine;
using UnityEngine.UIElements;
using CriminalCase2.Data;
using CriminalCase2.Services;
using CriminalCase2.ViewModels;

namespace CriminalCase2.UI
{
    public class SuspectDetailUI : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;

        private SuspectDetailViewModel? _viewModel;

        private Label _suspectNameLabel = null!;
        private Label _descriptionLabel = null!;
        private Label _evidenceTextLabel = null!;
        private Label _drugTestResultLabel = null!;
        private Button _drugTestButton = null!;
        private Button _verdictUserButton = null!;
        private Button _verdictDealerButton = null!;
        private Button _verdictNormalButton = null!;
        private Button _closeButton = null!;

        private bool _isBound;

        public void Populate(SuspectData suspect)
        {
            if (!_isBound) BindUI();

            DisposeViewModel();

            _viewModel = new SuspectDetailViewModel(suspect, GameServices.Levels!);
            _viewModel.StateChanged += OnViewModelStateChanged;
            _viewModel.VerdictRecorded += OnViewModelVerdictRecorded;
            _viewModel.CloseRequested += OnViewModelCloseRequested;
            OnViewModelStateChanged();
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
            DisposeViewModel();
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
                _verdictUserButton.text = SuspectRole.User.ToDisplayName();
                _verdictUserButton.clicked += OnVerdictUserClicked;
            }

            _verdictDealerButton = root.Q<Button>(UIConstants.SuspectDetail.VerdictDealerButton);
            if (_verdictDealerButton != null)
            {
                _verdictDealerButton.text = SuspectRole.Dealer.ToDisplayName();
                _verdictDealerButton.clicked += OnVerdictDealerClicked;
            }

            _verdictNormalButton = root.Q<Button>(UIConstants.SuspectDetail.VerdictNormalButton);
            if (_verdictNormalButton != null)
            {
                _verdictNormalButton.text = SuspectRole.Normal.ToDisplayName();
                _verdictNormalButton.clicked += OnVerdictNormalClicked;
            }

            _closeButton = root.Q<Button>(UIConstants.SuspectDetail.CloseButton);
            if (_closeButton != null)
            {
                _closeButton.clicked += OnCloseClicked;
            }

            _isBound = true;
        }

        private void UnbindUI()
        {
            if (_drugTestButton != null) _drugTestButton.clicked -= OnDrugTestClicked;
            if (_verdictUserButton != null) _verdictUserButton.clicked -= OnVerdictUserClicked;
            if (_verdictDealerButton != null) _verdictDealerButton.clicked -= OnVerdictDealerClicked;
            if (_verdictNormalButton != null) _verdictNormalButton.clicked -= OnVerdictNormalClicked;
            if (_closeButton != null) _closeButton.clicked -= OnCloseClicked;
            _isBound = false;
        }

        private void DisposeViewModel()
        {
            if (_viewModel == null) return;
            _viewModel.StateChanged -= OnViewModelStateChanged;
            _viewModel.VerdictRecorded -= OnViewModelVerdictRecorded;
            _viewModel.CloseRequested -= OnViewModelCloseRequested;
            _viewModel.Dispose();
            _viewModel = null;
        }

        private void OnViewModelStateChanged()
        {
            if (_viewModel == null) return;

            _suspectNameLabel.text = _viewModel.SuspectName;
            _descriptionLabel.text = _viewModel.Description;
            _evidenceTextLabel.text = _viewModel.EvidenceText;
            _drugTestResultLabel.text = _viewModel.DrugTestResultText;
            _drugTestButton.SetEnabled(_viewModel.IsDrugTestButtonEnabled);
        }

        private void OnViewModelVerdictRecorded()
        {
            GameServices.UI?.HideAllPanels();
            GameServices.UI?.ShowStatusHUD();
            GameServices.UI?.UpdateStatusHUD();
        }

        private void OnViewModelCloseRequested()
        {
            GameServices.UI?.HideAllPanels();
        }

        private void OnDrugTestClicked()
        {
            _viewModel?.RequestDrugTest();
        }

        private void OnVerdictUserClicked() => _viewModel?.SelectVerdict(SuspectRole.User);
        private void OnVerdictDealerClicked() => _viewModel?.SelectVerdict(SuspectRole.Dealer);
        private void OnVerdictNormalClicked() => _viewModel?.SelectVerdict(SuspectRole.Normal);

        private void OnCloseClicked()
        {
            _viewModel?.RequestClose();
        }
    }
}
