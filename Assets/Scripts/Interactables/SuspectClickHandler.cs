using UnityEngine;
using UnityEngine.EventSystems;
using CriminalCase2.Data;
using CriminalCase2.Services;
using CriminalCase2.UI;
using CriminalCase2.Utils;
using System.Threading;

namespace CriminalCase2.Interactables
{
    public class SuspectClickHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private SuspectData _suspectData;
        [SerializeField] private float _hoverScale = 1.1f;
        [SerializeField] private float _hoverRotationZ = 5f;
        [SerializeField] private float _hoverTweenDuration = 0.2f;

        private Vector3 _originalScale;
        private Quaternion _originalRotation;
        private CancellationTokenSource? _tweenCts;

        public SuspectData SuspectData => _suspectData;

        private void Awake()
        {
            _originalScale = transform.localScale;
            _originalRotation = transform.rotation;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_suspectData == null) return;

            var state = GameServices.GameState;
            if (state == null || state.CurrentState != GameState.Investigation) return;

            GameServices.UI?.ShowSuspectDetail(_suspectData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            var targetScale = _originalScale * _hoverScale;
            var targetRotation = _originalRotation * Quaternion.Euler(0, 0, _hoverRotationZ);
            StartTween(targetScale, targetRotation);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StartTween(_originalScale, _originalRotation);
        }

        private void StartTween(Vector3 targetScale, Quaternion targetRotation)
        {
            _tweenCts?.Cancel();
            _tweenCts?.Dispose();
            _tweenCts = new CancellationTokenSource();
            _ = TweenToAsync(targetScale, targetRotation, _tweenCts.Token);
        }

        private async Awaitable TweenToAsync(Vector3 targetScale, Quaternion targetRotation, CancellationToken token)
        {
            Vector3 startScale = transform.localScale;
            Quaternion startRotation = transform.rotation;
            float elapsed = 0f;

            while (elapsed < _hoverTweenDuration)
            {
                if (this == null || token.IsCancellationRequested) return;
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _hoverTweenDuration);
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                await Awaitable.NextFrameAsync(token);
            }

            if (this == null) return;
            transform.localScale = targetScale;
            transform.rotation = targetRotation;
        }

        private void OnDestroy()
        {
            _tweenCts?.Cancel();
            _tweenCts?.Dispose();
            _tweenCts = null;
        }

        private void OnValidate()
        {
            if (_suspectData == null)
            {
                GameLogger.Warn($"[SuspectClickHandler] No SuspectData assigned on {gameObject.name}.");
            }
        }
    }
}
