using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using CriminalCase2.Data;
using CriminalCase2.Managers;
using CriminalCase2.Utils;

namespace CriminalCase2.Interactables
{
    /// <summary>
    /// Handles player interaction with clues in the scene.
    /// Triggers discovery animation and notifies ClueManager.
    /// </summary>
    public class ClueClickHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private ClueData _clueData;
        [SerializeField] private float _foundScalePulse = 1.4f;
        [SerializeField] private float _pulseDuration = 0.3f;
        [SerializeField] private Color _foundColor = new Color(1f, 1f, 0.6f, 1f);
        [SerializeField] private float _hoverScale = 1.1f;

        private bool _isFound;
        private SpriteRenderer _spriteRenderer;
        private Color _originalColor;
        private Vector3 _originalScale;

        public ClueData Data => _clueData;
        public bool IsFound => _isFound;

        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (_spriteRenderer != null)
            {
                _originalColor = _spriteRenderer.color;
            }
            _originalScale = transform.localScale;
        }

        #region Pointer Event Handlers

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_clueData == null)
            {
                LoggingUtility.Error("Clue", $"No ClueData assigned on '{gameObject.name}'!");
                return;
            }

            if (_isFound) return;

            // Validate game state
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.ClueSearch)
            {
                return;
            }

            _isFound = true;
            StartCoroutine(FoundAnimationRoutine());

            // Notify ClueManager
            if (ClueManager.Instance == null)
            {
                LoggingUtility.Error("Clue", "ClueManager.Instance is null! Cannot register clue.");
                return;
            }

            ClueManager.Instance.OnClueFound(_clueData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isFound) return;
            ApplyHoverEffect();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isFound)
            {
                ResetVisuals();
            }
        }

        #endregion

        #region Visual Effects

        private void ApplyHoverEffect()
        {
            // Apply hover scale immediately (no Update() needed)
            transform.localScale = _originalScale * _hoverScale;
        }

        private void ResetVisuals()
        {
            transform.localScale = _originalScale;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _originalColor;
            }
        }

        private IEnumerator FoundAnimationRoutine()
        {
            Vector3 targetScale = _originalScale * _foundScalePulse;

            // Scale up + color change
            float elapsed = 0f;
            while (elapsed < _pulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _pulseDuration;
                transform.localScale = Vector3.Lerp(_originalScale, targetScale, t);
                if (_spriteRenderer != null)
                {
                    _spriteRenderer.color = Color.Lerp(_originalColor, _foundColor, t);
                }
                yield return null;
            }

            // Scale back + dim color
            elapsed = 0f;
            Color settledColor = new Color(_originalColor.r * 0.8f, _originalColor.g * 0.8f, _originalColor.b * 0.8f, 1f);
            while (elapsed < _pulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _pulseDuration;
                transform.localScale = Vector3.Lerp(targetScale, _originalScale, t);
                if (_spriteRenderer != null)
                {
                    _spriteRenderer.color = Color.Lerp(_foundColor, settledColor, t);
                }
                yield return null;
            }

            // Final state
            transform.localScale = _originalScale;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = settledColor;
            }
        }

        #endregion

        #region Public Methods

        public void ResetClue()
        {
            _isFound = false;
            ResetVisuals();
        }

        #endregion

        #region Validation

        private void OnValidate()
        {
            if (_clueData == null)
            {
                LoggingUtility.Warning("Clue", $"No ClueData assigned on {gameObject.name}.");
            }
        }

        #endregion
    }
}
