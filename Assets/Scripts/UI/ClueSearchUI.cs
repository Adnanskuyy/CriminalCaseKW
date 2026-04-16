using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using CriminalCase2.Data;
using CriminalCase2.Managers;
using CriminalCase2.Services.Interfaces;
using CriminalCase2.Utils;

namespace CriminalCase2.UI
{
    /// <summary>
    /// UI Toolkit-based clue search interface.
    /// Displays found clues and manages clue inventory UI.
    /// </summary>
    public class ClueSearchUI : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;

        private IClueService _clueService;
        private Label _counterLabel;
        private VisualElement _inventory;
        private Button _proceedButton;
        private bool _isBound;

        private readonly List<VisualElement> _iconSlots = new List<VisualElement>();

        #region Unity Lifecycle

        private void OnEnable()
        {
            BindUI();
            SubscribeToClueService();
        }

        private void OnDisable()
        {
            UnsubscribeFromClueService();
            UnbindUI();
        }

        #endregion

        #region UI Binding

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
                _proceedButton.style.display = DisplayStyle.None;
            }

            _isBound = true;
            LoggingUtility.LogUI("ClueSearchUI bound successfully", LogLevel.Debug);
        }

        private void UnbindUI()
        {
            if (_proceedButton != null)
            {
                _proceedButton.clicked -= OnProceedClicked;
            }
            _isBound = false;
        }

        #endregion

        #region Service Integration

        private void SubscribeToClueService()
        {
            // Get service from ServiceLocator
            _clueService = ServiceLocator.Get<IClueService>();

            // If not available yet, wait for it
            if (_clueService == null)
            {
                ServiceLocator.WhenAvailable<IClueService>(service =>
                {
                    _clueService = service;
                    RegisterClueEvents();
                    RefreshUI();
                });
            }
            else
            {
                RegisterClueEvents();
                RefreshUI();
            }
        }

        private void RegisterClueEvents()
        {
            if (_clueService == null) return;

            _clueService.OnClueFound += OnClueFound;
            _clueService.OnAllCluesFound += OnAllCluesFound;
            _clueService.OnCluesInitialized += OnCluesInitialized;

            LoggingUtility.LogUI("Subscribed to ClueService events", LogLevel.Debug);
        }

        private void UnsubscribeFromClueService()
        {
            if (_clueService == null) return;

            _clueService.OnClueFound -= OnClueFound;
            _clueService.OnAllCluesFound -= OnAllCluesFound;
            _clueService.OnCluesInitialized -= OnCluesInitialized;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the UI with level clues.
        /// Called by UIManager when showing the clue search panel.
        /// </summary>
        public void Initialize(ClueData[] clues)
        {
            if (!_isBound) BindUI();
            if (_inventory == null) return;

            // Clear existing slots
            _inventory.Clear();
            _iconSlots.Clear();

            if (clues == null) return;

            // Create slots for each clue
            for (int i = 0; i < clues.Length; i++)
            {
                var slot = CreateClueSlot(clues[i]);
                _inventory.Add(slot);
                _iconSlots.Add(slot);
            }

            UpdateCounter(0, clues.Length);

            // Hide proceed button initially
            if (_proceedButton != null)
            {
                _proceedButton.style.display = DisplayStyle.None;
            }

            LoggingUtility.LogUI($"Initialized with {clues.Length} clue slots");
        }

        private VisualElement CreateClueSlot(ClueData clue)
        {
            // Main slot container
            var slot = new VisualElement();
            slot.AddToClassList("clue-icon-slot");
            slot.userData = clue;

            // Content container
            var content = new VisualElement();
            content.AddToClassList("clue-slot-content");
            slot.Add(content);

            // Silhouette background
            var silhouetteBg = new VisualElement();
            silhouetteBg.AddToClassList("clue-silhouette-bg");
            content.Add(silhouetteBg);

            // Silhouette icon
            var silhouette = new VisualElement();
            silhouette.AddToClassList("clue-silhouette");
            silhouetteBg.Add(silhouette);

            // Set silhouette sprite
            if (clue.ClueIcon != null)
            {
                silhouette.style.backgroundImage = new StyleBackground(clue.ClueIcon);
            }

            // Clue name label
            var nameLabel = new Label(clue.ClueName);
            nameLabel.AddToClassList("clue-hint-label");
            content.Add(nameLabel);

            return slot;
        }

        #endregion

        #region Event Handlers

        private void OnCluesInitialized(ClueData[] clues)
        {
            Initialize(clues);
        }

        private void OnClueFound(ClueData clue)
        {
            LoggingUtility.LogClue($"UI received clue found event: {clue?.ClueName}");

            if (clue == null) return;

            RefreshUI();
            UpdateIconSlot(clue);
        }

        private void OnAllCluesFound()
        {
            LoggingUtility.LogClue("All clues found - showing proceed button");
            ShowProceedButton();
        }

        private void OnProceedClicked()
        {
            if (_clueService?.AllCluesFound == true)
            {
                GameManager.Instance?.SetState(GameState.Deduction);
            }
        }

        #endregion

        #region UI Updates

        private void RefreshUI()
        {
            if (_clueService == null) return;

            UpdateCounter(_clueService.FoundCount, _clueService.TotalCount);

            // Update all found clue slots
            foreach (var clue in _clueService.FoundClues)
            {
                UpdateIconSlot(clue);
            }

            // Show proceed button if all clues found
            if (_clueService.AllCluesFound)
            {
                ShowProceedButton();
            }
        }

        private void UpdateIconSlot(ClueData foundClue)
        {
            if (foundClue == null) return;

            foreach (var slot in _iconSlots)
            {
                var clue = slot.userData as ClueData;
                if (clue == foundClue)
                {
                    // Mark as found
                    slot.AddToClassList("found");

                    // Get content container
                    var content = slot.Q(className: "clue-slot-content");
                    if (content == null) continue;

                    // Clear and rebuild as found state
                    content.Clear();

                    var foundContainer = new VisualElement();
                    foundContainer.AddToClassList("clue-found-container");
                    content.Add(foundContainer);

                    // Add icon
                    if (foundClue.ClueIcon != null)
                    {
                        var iconContainer = new VisualElement();
                        iconContainer.AddToClassList("clue-icon-container");
                        foundContainer.Add(iconContainer);

                        var iconImage = new VisualElement();
                        iconImage.AddToClassList("clue-icon-image");
                        iconImage.style.backgroundImage = new StyleBackground(foundClue.ClueIcon);
                        iconContainer.Add(iconImage);
                    }

                    // Add label
                    var foundLabel = new Label(foundClue.ClueName);
                    foundLabel.AddToClassList("clue-found-label");
                    foundContainer.Add(foundLabel);

                    // Add checkmark badge
                    var foundBadge = new VisualElement();
                    foundBadge.AddToClassList("clue-found-badge");
                    var checkmark = new Label("✓");
                    checkmark.AddToClassList("clue-found-checkmark");
                    foundBadge.Add(checkmark);
                    slot.Add(foundBadge);

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

        #endregion
    }
}
