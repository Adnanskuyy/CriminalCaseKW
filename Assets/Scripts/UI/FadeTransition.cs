using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections;

namespace CriminalCase2.UI
{
    public class FadeTransition : MonoBehaviour
    {
        [Header("UI Toolkit")]
        [SerializeField] private UIDocument _uiDocument;

        [Header("Fade Settings")]
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private Color _fadeColor = Color.black;

        private VisualElement _fadePanel;
        private bool _isFading;

        public bool IsFading => _isFading;
        public float FadeDuration => _fadeDuration;

        private void Awake()
        {
            if (_uiDocument == null)
            {
                _uiDocument = GetComponent<UIDocument>();
            }

            CreateFadePanel();
        }

        private void CreateFadePanel()
        {
            if (_uiDocument == null) return;

            var root = _uiDocument.rootVisualElement;
            if (root == null) return;

            // Create fade panel
            _fadePanel = new VisualElement
            {
                name = "fade-panel",
                style =
                {
                    position = Position.Absolute,
                    left = 0,
                    top = 0,
                    right = 0,
                    bottom = 0,
                    backgroundColor = _fadeColor,
                    opacity = 0,
                    display = DisplayStyle.None
                }
            };

            // Add to root (make sure it's on top)
            _fadePanel.AddToClassList("fade-panel");
            root.Add(_fadePanel);
        }

        /// <summary>
        /// Fade in (screen becomes black)
        /// </summary>
        public void FadeIn(Action onComplete = null)
        {
            if (_fadePanel == null || _isFading) return;
            
            StopAllCoroutines();
            StartCoroutine(FadeInCoroutine(onComplete));
        }

        /// <summary>
        /// Fade out (screen becomes clear)
        /// </summary>
        public void FadeOut(Action onComplete = null)
        {
            if (_fadePanel == null || _isFading) return;
            
            StopAllCoroutines();
            StartCoroutine(FadeOutCoroutine(onComplete));
        }

        /// <summary>
        /// Fade in then out (for level transitions)
        /// </summary>
        public void FadeInOut(Action onMiddle = null, Action onComplete = null)
        {
            if (_fadePanel == null || _isFading) return;
            
            StopAllCoroutines();
            StartCoroutine(FadeInOutCoroutine(onMiddle, onComplete));
        }

        private IEnumerator FadeInCoroutine(Action onComplete)
        {
            _isFading = true;
            _fadePanel.style.display = DisplayStyle.Flex;

            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _fadeDuration;
                _fadePanel.style.opacity = t;
                yield return null;
            }

            _fadePanel.style.opacity = 1f;
            _isFading = false;
            onComplete?.Invoke();
        }

        private IEnumerator FadeOutCoroutine(Action onComplete)
        {
            _isFading = true;
            _fadePanel.style.display = DisplayStyle.Flex;

            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = 1f - (elapsed / _fadeDuration);
                _fadePanel.style.opacity = t;
                yield return null;
            }

            _fadePanel.style.opacity = 0f;
            _fadePanel.style.display = DisplayStyle.None;
            _isFading = false;
            onComplete?.Invoke();
        }

        private IEnumerator FadeInOutCoroutine(Action onMiddle, Action onComplete)
        {
            _isFading = true;
            _fadePanel.style.display = DisplayStyle.Flex;

            // Fade in
            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _fadeDuration;
                _fadePanel.style.opacity = t;
                yield return null;
            }

            _fadePanel.style.opacity = 1f;
            
            // Middle callback (switch levels here)
            onMiddle?.Invoke();
            
            // Small delay when fully black
            yield return new WaitForSeconds(0.1f);

            // Fade out
            elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = 1f - (elapsed / _fadeDuration);
                _fadePanel.style.opacity = t;
                yield return null;
            }

            _fadePanel.style.opacity = 0f;
            _fadePanel.style.display = DisplayStyle.None;
            _isFading = false;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Instantly set fade state (for initialization)
        /// </summary>
        public void SetFadeInstant(float opacity)
        {
            if (_fadePanel == null) return;
            
            StopAllCoroutines();
            _fadePanel.style.opacity = opacity;
            _fadePanel.style.display = opacity > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            _isFading = false;
        }
    }
}
