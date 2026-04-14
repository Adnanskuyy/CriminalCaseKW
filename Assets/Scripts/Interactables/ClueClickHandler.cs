using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using CriminalCase2.Data;
using CriminalCase2.Managers;

namespace CriminalCase2.Interactables
{
    public class ClueClickHandler : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private ClueData _clueData;
        [SerializeField] private float _foundScalePulse = 1.4f;
        [SerializeField] private float _pulseDuration = 0.3f;
        [SerializeField] private Color _foundColor = new Color(1f, 1f, 0.6f, 1f);

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

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_clueData == null) return;
            if (_isFound) return;

            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.ClueSearch) return;

            _isFound = true;
            StartCoroutine(FoundAnimationRoutine());

            ClueManager.Instance?.OnClueFound(_clueData);
        }

        public void ResetClue()
        {
            _isFound = false;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _originalColor;
            }
            transform.localScale = _originalScale;
        }

        private IEnumerator FoundAnimationRoutine()
        {
            Vector3 targetScale = _originalScale * _foundScalePulse;

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

            elapsed = 0f;
            while (elapsed < _pulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _pulseDuration;
                transform.localScale = Vector3.Lerp(targetScale, _originalScale, t);
                if (_spriteRenderer != null)
                {
                    Color settledColor = new Color(_originalColor.r * 0.8f, _originalColor.g * 0.8f, _originalColor.b * 0.8f, 1f);
                    _spriteRenderer.color = Color.Lerp(_foundColor, settledColor, t);
                }
                yield return null;
            }

            transform.localScale = _originalScale;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = new Color(_originalColor.r * 0.8f, _originalColor.g * 0.8f, _originalColor.b * 0.8f, 1f);
            }
        }

        private void OnValidate()
        {
            if (_clueData == null)
            {
                Debug.LogWarning($"[ClueClickHandler] No ClueData assigned on {gameObject.name}.");
            }
        }
    }
}