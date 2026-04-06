using UnityEngine;
using UnityEngine.EventSystems;
using CriminalCase2.Data;
using CriminalCase2.Managers;
using CriminalCase2.UI;

namespace CriminalCase2.Interactables
{
    public class SuspectClickHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private SuspectData _suspectData;
        [SerializeField] private float _hoverScale = 1.1f;
        [SerializeField] private float _hoverRotationZ = 5f;

        private Vector3 _originalScale;
        private Quaternion _originalRotation;
        private bool _isHovering;

        public SuspectData SuspectData => _suspectData;

        private void Awake()
        {
            _originalScale = transform.localScale;
            _originalRotation = transform.rotation;
        }

        private void Update()
        {
            if (_isHovering)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, _originalScale * _hoverScale, Time.deltaTime * 10f);
                var targetRotation = _originalRotation * Quaternion.Euler(0, 0, _hoverRotationZ);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
            else
            {
                transform.localScale = Vector3.Lerp(transform.localScale, _originalScale, Time.deltaTime * 10f);
                transform.rotation = Quaternion.Lerp(transform.rotation, _originalRotation, Time.deltaTime * 10f);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_suspectData == null) return;

            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Investigation) return;

            UIManager.Instance?.ShowSuspectDetail(_suspectData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovering = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovering = false;
        }

        private void OnValidate()
        {
            if (_suspectData == null)
            {
                Debug.LogWarning($"[SuspectClickHandler] No SuspectData assigned on {gameObject.name}.");
            }
        }
    }
}
