using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using CriminalCase2.Data;
using CriminalCase2.Managers;

namespace CriminalCase2.UI
{
    public class ClueSearchUI : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;

        private Label _counterLabel;
        private VisualElement _inventory;
        private Button _proceedButton;
        private bool _isBound;

        private readonly List<VisualElement> _iconSlots = new List<VisualElement>();

        public void Initialize(ClueData[] clues)
        {
            if (!_isBound) BindUI();
            if (_inventory == null) return;

            _inventory.Clear();
            _iconSlots.Clear();

            if (clues == null) return;

            for (int i = 0; i < clues.Length; i++)
            {
                var slot = CreateClueSlot(clues[i]);
                _inventory.Add(slot);
                _iconSlots.Add(slot);
            }

            UpdateCounter(0, clues.Length);

            if (_proceedButton != null)
            {
                _proceedButton.style.display = DisplayStyle.None;
            }
        }

        public void OnClueFound(ClueData clue)
        {
            if (!_isBound) BindUI();

            int foundCount = ClueManager.Instance != null ? ClueManager.Instance.FoundCount : 0;
            int totalCount = ClueManager.Instance != null ? ClueManager.Instance.TotalClueCount : 0;

            UpdateCounter(foundCount, totalCount);
            UpdateIconSlot(clue);

            if (ClueManager.Instance != null && ClueManager.Instance.AllCluesFound)
            {
                ShowProceedButton();
            }
        }

        private void OnEnable()
        {
            BindUI();
            if (ClueManager.Instance != null)
            {
                ClueManager.Instance.OnClueFoundEvent += OnClueFound;
                ClueManager.Instance.OnAllCluesFoundEvent += OnAllCluesFound;
            }
        }

        private void OnDisable()
        {
            if (ClueManager.Instance != null)
            {
                ClueManager.Instance.OnClueFoundEvent -= OnClueFound;
                ClueManager.Instance.OnAllCluesFoundEvent -= OnAllCluesFound;
            }
            UnbindUI();
        }

        private void BindUI()
        {
            if (_document == null || _isBound) return;

            var root = _document.rootVisualElement;
            if (root == null) return;

            _counterLabel = root.Q<Label>("clue-counter-label");
            _inventory = root.Q<VisualElement>("clue-inventory");
            _proceedButton = root.Q<Button>("proceed-to-deduction-button");

            if (_proceedButton != null)
            {
                _proceedButton.clicked += OnProceedClicked;
            }

            _isBound = true;
        }

        private void UnbindUI()
        {
            if (_proceedButton != null)
            {
                _proceedButton.clicked -= OnProceedClicked;
            }
            _isBound = false;
        }

        private VisualElement CreateClueSlot(ClueData clue)
        {
            var slot = new VisualElement();
            slot.AddToClassList("clue-icon-slot");

            var silhouette = new VisualElement();
            silhouette.AddToClassList("clue-icon-silhouette");
            slot.Add(silhouette);

            var nameLabel = new Label("?");
            nameLabel.AddToClassList("clue-icon-label");
            slot.Add(nameLabel);

            slot.userData = clue;
            return slot;
        }

        private void UpdateIconSlot(ClueData foundClue)
        {
            foreach (var slot in _iconSlots)
            {
                var clue = slot.userData as ClueData;
                if (clue == foundClue)
                {
                    slot.AddToClassList("found");

                    slot.Clear();

                    if (foundClue.ClueIcon != null)
                    {
                        var iconImage = new VisualElement();
                        iconImage.AddToClassList("clue-icon-image");
                        iconImage.style.backgroundImage = new StyleBackground(foundClue.ClueIcon);
                        slot.Add(iconImage);
                    }

                    var nameLabel = new Label(foundClue.ClueName);
                    nameLabel.AddToClassList("clue-icon-label");
                    slot.Add(nameLabel);

                    break;
                }
            }
        }

        private void UpdateCounter(int found, int total)
        {
            if (_counterLabel != null)
            {
                _counterLabel.text = $"Petunjuk Ditemukan: {found}/{total}";
            }
        }

        private void ShowProceedButton()
        {
            if (_proceedButton != null)
            {
                _proceedButton.style.display = DisplayStyle.Flex;
            }
        }

        private void OnAllCluesFound()
        {
            ShowProceedButton();
        }

        private void OnProceedClicked()
        {
            if (ClueManager.Instance != null && ClueManager.Instance.AllCluesFound)
            {
                GameManager.Instance?.SetState(GameState.Deduction);
            }
        }
    }
}