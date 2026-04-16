using UnityEngine;
using UnityEngine.EventSystems;
using CriminalCase2.Data;
using CriminalCase2.Managers;
using CriminalCase2.UI;
using CriminalCase2.Utils;

namespace CriminalCase2.Interactables
{
    /// <summary>
    /// Handles player interaction with suspects in the scene.
    /// Shows hover effects and opens suspect detail UI on click.
    /// </summary>
    public class SuspectClickHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private SuspectData _suspectData;
        [SerializeField] private float _hoverScale = 1.1f;

        private Vector3 _originalScale;

        public SuspectData SuspectData => _suspectData;

        private void Awake()
        {
            _originalScale = transform.localScale;
        }

        #region Pointer Event Handlers

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_suspectData == null)
            {
                LoggingUtility.Warning("Suspect", "No SuspectData assigned");
                return;
            }

            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.RoleAssignment)
            {
                return;
            }

            LoggingUtility.LogUI($"Clicked on suspect: {_suspectData.SuspectName}");
            UIManager.Instance?.ShowSuspectDetail(_suspectData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ApplyHoverEffect();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ResetVisuals();
        }

        #endregion

        #region Visual Effects

        private void ApplyHoverEffect()
        {
            // Immediate hover effect - no Update() needed
            transform.localScale = _originalScale * _hoverScale;
        }

        private void ResetVisuals()
        {
            transform.localScale = _originalScale;
        }

        #endregion

        #region Validation

        private void OnValidate()
        {
            if (_suspectData == null)
            {
                LoggingUtility.Warning("Suspect", $"No SuspectData assigned on {gameObject.name}.");
            }
        }

        #endregion
    }
}
