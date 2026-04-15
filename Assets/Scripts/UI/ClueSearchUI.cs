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
        private bool _isSubscribed;

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
            Debug.Log($"[ClueSearchUI] OnClueFound() called for clue: {(clue != null ? clue.ClueName : "NULL")}");

            if (!_isBound) BindUI();

            int foundCount = ClueManager.Instance != null ? ClueManager.Instance.FoundCount : 0;
            int totalCount = ClueManager.Instance != null ? ClueManager.Instance.TotalClueCount : 0;

            Debug.Log($"[ClueSearchUI] Updating counter to: {foundCount}/{totalCount}");
            UpdateCounter(foundCount, totalCount);
            UpdateIconSlot(clue);

            if (ClueManager.Instance != null && ClueManager.Instance.AllCluesFound)
            {
                ShowProceedButton();
            }
        }

        private void OnEnable()
        {
            Debug.Log("[ClueSearchUI] OnEnable() called.");
            BindUI();
            SubscribeToEvents();
            Debug.Log($"[ClueSearchUI] OnEnable complete. _isSubscribed={_isSubscribed}, ClueManager.Instance={(ClueManager.Instance != null ? "exists" : "null")}");
        }

        private void Update()
        {
            // Retry subscription if ClueManager became available after OnEnable
            if (!_isSubscribed && ClueManager.Instance != null)
            {
                Debug.Log("[ClueSearchUI] Update() - Retrying event subscription...");
                SubscribeToEvents();
            }
        }

        private void SubscribeToEvents()
        {
            if (ClueManager.Instance == null)
            {
                Debug.Log("[ClueSearchUI] SubscribeToEvents() - ClueManager.Instance is null, cannot subscribe.");
                return;
            }
            if (_isSubscribed)
            {
                Debug.Log("[ClueSearchUI] SubscribeToEvents() - Already subscribed.");
                return;
            }

            Debug.Log("[ClueSearchUI] SubscribeToEvents() - Subscribing to ClueManager events...");
            ClueManager.Instance.OnClueFoundEvent += OnClueFound;
            ClueManager.Instance.OnAllCluesFoundEvent += OnAllCluesFound;
            _isSubscribed = true;
            Debug.Log("[ClueSearchUI] SubscribeToEvents() - Subscription complete!");
        }

        private void OnDisable()
        {
            Debug.Log("[ClueSearchUI] OnDisable() called - unsubscribing from events.");
            if (ClueManager.Instance != null)
            {
                ClueManager.Instance.OnClueFoundEvent -= OnClueFound;
                ClueManager.Instance.OnAllCluesFoundEvent -= OnAllCluesFound;
            }
            _isSubscribed = false;
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
            _isSubscribed = false;
        }

        private VisualElement CreateClueSlot(ClueData clue)
        {
            Debug.Log($"[ClueSearchUI] Creating slot for clue: {clue?.ClueName}, Icon: {(clue?.ClueIcon != null ? "YES" : "NULL")}");

            // Main slot container
            var slot = new VisualElement();
            slot.AddToClassList("clue-icon-slot");
            slot.style.display = DisplayStyle.Flex;
            slot.userData = clue;

            // Content container for silhouette/icon and label
            var content = new VisualElement();
            content.AddToClassList("clue-slot-content");
            content.style.display = DisplayStyle.Flex;
            slot.Add(content);

            // Silhouette background (distinguishable background)
            var silhouetteBg = new VisualElement();
            silhouetteBg.AddToClassList("clue-silhouette-bg");
            silhouetteBg.style.display = DisplayStyle.Flex;
            content.Add(silhouetteBg);

            // Silhouette icon (darkened version of clue)
            var silhouette = new VisualElement();
            silhouette.AddToClassList("clue-silhouette");
            silhouette.style.display = DisplayStyle.Flex;
            silhouetteBg.Add(silhouette);

            // Set the silhouette sprite if clue has an icon
            if (clue.ClueIcon != null)
            {
                silhouette.style.backgroundImage = new StyleBackground(clue.ClueIcon);
                Debug.Log($"[ClueSearchUI] Set silhouette image for: {clue.ClueName}");
            }
            else
            {
                Debug.LogWarning($"[ClueSearchUI] No ClueIcon for: {clue.ClueName}");
            }

            // Clue name label (shown as hint)
            var nameLabel = new Label(clue.ClueName);
            nameLabel.AddToClassList("clue-hint-label");
            nameLabel.style.display = DisplayStyle.Flex;
            content.Add(nameLabel);

            Debug.Log($"[ClueSearchUI] Slot created successfully for: {clue.ClueName}");

            return slot;
        }

        private void UpdateIconSlot(ClueData foundClue)
        {
            Debug.Log($"[ClueSearchUI] UpdateIconSlot called for: {foundClue?.ClueName}");

            foreach (var slot in _iconSlots)
            {
                var clue = slot.userData as ClueData;
                if (clue == foundClue)
                {
                    Debug.Log($"[ClueSearchUI] Found matching slot for: {foundClue.ClueName}");

                    // Add found class for styling
                    slot.AddToClassList("found");

                    // Get the content container
                    var content = slot.Q(className: "clue-slot-content");
                    if (content == null)
                    {
                        Debug.LogError("[ClueSearchUI] content container not found!");
                        continue;
                    }

                    // Clear the content
                    content.Clear();

                    // Create found state container
                    var foundContainer = new VisualElement();
                    foundContainer.AddToClassList("clue-found-container");
                    foundContainer.style.display = DisplayStyle.Flex;
                    content.Add(foundContainer);

                    // Add the actual clue icon
                    if (foundClue.ClueIcon != null)
                    {
                        var iconContainer = new VisualElement();
                        iconContainer.AddToClassList("clue-icon-container");
                        iconContainer.style.display = DisplayStyle.Flex;
                        foundContainer.Add(iconContainer);

                        var iconImage = new VisualElement();
                        iconImage.AddToClassList("clue-icon-image");
                        iconImage.style.display = DisplayStyle.Flex;
                        iconImage.style.backgroundImage = new StyleBackground(foundClue.ClueIcon);
                        iconContainer.Add(iconImage);

                        Debug.Log($"[ClueSearchUI] Set found icon for: {foundClue.ClueName}");
                    }

                    // Add found label with actual clue name
                    var foundLabel = new Label(foundClue.ClueName);
                    foundLabel.AddToClassList("clue-found-label");
                    foundLabel.style.display = DisplayStyle.Flex;
                    foundContainer.Add(foundLabel);

                    // Add found badge
                    var foundBadge = new VisualElement();
                    foundBadge.AddToClassList("clue-found-badge");
                    foundBadge.style.display = DisplayStyle.Flex;
                    var checkmark = new Label("✓");
                    checkmark.AddToClassList("clue-found-checkmark");
                    checkmark.style.display = DisplayStyle.Flex;
                    foundBadge.Add(checkmark);
                    slot.Add(foundBadge);

                    Debug.Log($"[ClueSearchUI] Slot updated successfully for: {foundClue.ClueName}");

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
