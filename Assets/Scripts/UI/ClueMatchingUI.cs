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
    public class ClueMatchingUI : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;

        private IClueMatchingService _matchingService;

        private Label _counterLabel;
        private VisualElement _suspectCardsRow;
        private VisualElement _cluePool;
        private Button _confirmButton;

        private readonly VisualElement[] _suspectCards = new VisualElement[4];
        private readonly VisualElement[] _suspectCardClueContainers = new VisualElement[4];
        private readonly Label[] _suspectCardNames = new Label[4];
        private readonly VisualElement[] _suspectCardPortraits = new VisualElement[4];

        private ClueData _selectedClue;
        private SuspectData[] _suspects;

        private bool _isBound;

        public void Initialize(ClueData[] clues, SuspectData[] suspects)
        {
            EnsureMatchingService();
            _matchingService.Initialize(clues, suspects);
            _suspects = suspects;
            _selectedClue = null;

            if (!_isBound) BindUI();

            UnbindFromService();
            SubscribeToService();

            BuildSuspectCards(suspects);
            BuildCluePool(clues);
            UpdateCounter();
            UpdateConfirmButton();
        }

        private void OnEnable()
        {
            if (_document != null && _document.rootVisualElement != null)
                BindUI();
        }

        private void OnDisable()
        {
            UnbindFromService();
            _isBound = false;
        }

        private void EnsureMatchingService()
        {
            if (_matchingService != null) return;

            _matchingService = ServiceLocator.Get<IClueMatchingService>();
            if (_matchingService == null)
            {
                _matchingService = new ClueMatchingService();
                ServiceLocator.Register<IClueMatchingService>(_matchingService);
            }
        }

        private void BindUI()
        {
            if (_document == null || _isBound) return;

            var root = _document.rootVisualElement;
            if (root == null) return;

            _counterLabel = root.Q<Label>("clue-matching-counter");
            _suspectCardsRow = root.Q<VisualElement>("suspect-cards-row");
            _cluePool = root.Q<VisualElement>("clue-pool");
            _confirmButton = root.Q<Button>("confirm-matching-button");

            if (_confirmButton != null)
                _confirmButton.clicked += OnConfirmClicked;

            for (int i = 0; i < 4; i++)
            {
                _suspectCards[i] = root.Q<VisualElement>($"suspect-card-{i}");
                _suspectCardClueContainers[i] = root.Q<VisualElement>($"suspect-card-clues-{i}");
                _suspectCardNames[i] = root.Q<Label>($"suspect-card-name-{i}");
                _suspectCardPortraits[i] = root.Q<VisualElement>($"suspect-card-portrait-{i}");

                if (_suspectCards[i] != null)
                {
                    int index = i;
                    _suspectCards[i].RegisterCallback<PointerDownEvent>(evt => OnSuspectCardClicked(index));
                }
            }

            SubscribeToService();
            _isBound = true;
        }

        private void SubscribeToService()
        {
            if (_matchingService == null) return;
            _matchingService.OnClueAssigned += HandleClueAssigned;
            _matchingService.OnClueUnassigned += HandleClueUnassigned;
            _matchingService.OnAllCluesAssigned += HandleAllCluesAssigned;
        }

        private void UnbindFromService()
        {
            if (_matchingService == null) return;
            _matchingService.OnClueAssigned -= HandleClueAssigned;
            _matchingService.OnClueUnassigned -= HandleClueUnassigned;
            _matchingService.OnAllCluesAssigned -= HandleAllCluesAssigned;
        }

        private void BuildSuspectCards(SuspectData[] suspects)
        {
            for (int i = 0; i < 4; i++)
            {
                if (i < suspects.Length && suspects[i] != null)
                {
                    if (_suspectCardNames[i] != null)
                        _suspectCardNames[i].text = suspects[i].SuspectName;

                    if (_suspectCardPortraits[i] != null)
                    {
                        var initialLabel = _suspectCardPortraits[i].Q<Label>($"suspect-card-initial-{i}");
                        if (initialLabel != null && suspects[i].SuspectName.Length > 0)
                            initialLabel.text = suspects[i].SuspectName.Substring(0, 1).ToUpper();
                    }
                }

                if (_suspectCardClueContainers[i] != null)
                    _suspectCardClueContainers[i].Clear();
            }
        }

        private void BuildCluePool(ClueData[] clues)
        {
            if (_cluePool == null) return;
            _cluePool.Clear();

            if (clues == null) return;

            foreach (var clue in clues)
            {
                var item = new VisualElement();
                item.AddToClassList("clue-pool-item");
                item.userData = clue;

                if (clue.ClueIcon != null)
                {
                    var icon = new VisualElement();
                    icon.AddToClassList("clue-pool-item-image");
                    icon.style.backgroundImage = new StyleBackground(clue.ClueIcon);
                    item.Add(icon);
                }
                else
                {
                    var nameLabel = new Label(clue.ClueName);
                    nameLabel.AddToClassList("clue-pool-item-label");
                    item.Add(nameLabel);
                }

                item.RegisterCallback<PointerDownEvent>(evt => OnCluePoolItemClicked(clue, item));
                _cluePool.Add(item);
            }
        }

        private void OnCluePoolItemClicked(ClueData clue, VisualElement item)
        {
            if (_matchingService.IsClueAssigned(clue)) return;

            if (_selectedClue == clue)
            {
                DeselectClue();
                return;
            }

            DeselectClue();
            _selectedClue = clue;
            item.AddToClassList("selected");
        }

        private void OnSuspectCardClicked(int suspectIndex)
        {
            if (_suspects == null || suspectIndex >= _suspects.Length) return;
            if (_selectedClue == null) return;

            var suspect = _suspects[suspectIndex];
            _matchingService.AssignClue(_selectedClue, suspect);
            DeselectClue();
        }

        private void OnClueOnCardClicked(ClueData clue, int suspectIndex)
        {
            _matchingService.UnassignClue(clue);
        }

        private void DeselectClue()
        {
            _selectedClue = null;
            if (_cluePool == null) return;

            foreach (var child in _cluePool.Children())
            {
                child.RemoveFromClassList("selected");
            }
        }

        private void HandleClueAssigned(ClueData clue, SuspectData suspect)
        {
            HideClueFromPool(clue);
            AddClueToSuspectCard(clue, suspect);
            UpdateCounter();
            UpdateConfirmButton();
        }

        private void HandleClueUnassigned(ClueData clue)
        {
            RemoveClueFromSuspectCards(clue);
            ShowClueInPool(clue);
            UpdateCounter();
            UpdateConfirmButton();
        }

        private void HandleAllCluesAssigned()
        {
            UpdateConfirmButton();
        }

        private void HideClueFromPool(ClueData clue)
        {
            if (_cluePool == null) return;

            foreach (var child in _cluePool.Children())
            {
                if (child.userData is ClueData poolClue && poolClue == clue)
                {
                    child.style.display = DisplayStyle.None;
                    break;
                }
            }
        }

        private void ShowClueInPool(ClueData clue)
        {
            if (_cluePool == null) return;

            foreach (var child in _cluePool.Children())
            {
                if (child.userData is ClueData poolClue && poolClue == clue)
                {
                    child.style.display = DisplayStyle.Flex;
                    child.RemoveFromClassList("selected");
                    break;
                }
            }
        }

        private void AddClueToSuspectCard(ClueData clue, SuspectData suspect)
        {
            int index = FindSuspectIndex(suspect);
            if (index < 0 || _suspectCardClueContainers[index] == null) return;

            var icon = new VisualElement();
            icon.AddToClassList("suspect-card-clue-icon");
            icon.userData = clue;

            if (clue.ClueIcon != null)
            {
                var img = new VisualElement();
                img.AddToClassList("suspect-card-clue-icon-image");
                img.style.backgroundImage = new StyleBackground(clue.ClueIcon);
                icon.Add(img);
            }

            int capturedIndex = index;
            icon.RegisterCallback<PointerDownEvent>(evt =>
            {
                evt.StopPropagation();
                OnClueOnCardClicked(clue, capturedIndex);
            });

            _suspectCardClueContainers[index].Add(icon);
        }

        private void RemoveClueFromSuspectCards(ClueData clue)
        {
            for (int i = 0; i < 4; i++)
            {
                if (_suspectCardClueContainers[i] == null) continue;

                VisualElement toRemove = null;
                foreach (var child in _suspectCardClueContainers[i].Children())
                {
                    if (child.userData is ClueData cardClue && cardClue == clue)
                    {
                        toRemove = child;
                        break;
                    }
                }

                if (toRemove != null)
                {
                    _suspectCardClueContainers[i].Remove(toRemove);
                    break;
                }
            }
        }

        private int FindSuspectIndex(SuspectData suspect)
        {
            if (_suspects == null) return -1;
            for (int i = 0; i < _suspects.Length; i++)
            {
                if (_suspects[i] == suspect) return i;
            }
            return -1;
        }

        private void UpdateCounter()
        {
            if (_counterLabel == null || _matchingService == null) return;
            int assigned = _matchingService.AllClues.Count - _matchingService.UnassignedClues.Count;
            _counterLabel.text = $"Petunjuk Terpasang: {assigned}/{_matchingService.TotalClueCount}";
        }

        private void UpdateConfirmButton()
        {
            if (_confirmButton == null || _matchingService == null) return;
            _confirmButton.SetEnabled(_matchingService.AllCluesAssigned);
        }

        private void OnConfirmClicked()
        {
            if (_matchingService == null || !_matchingService.AllCluesAssigned) return;
            _matchingService.ConfirmMatchings();
            GameManager.Instance?.SetState(GameState.RoleAssignment);
        }
    }
}
