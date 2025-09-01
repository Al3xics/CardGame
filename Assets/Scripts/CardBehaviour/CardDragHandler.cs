using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Wendogo
{
    /// <summary>
    /// Handles drag-and-drop behavior for card objects in the UI.
    /// Coordinates with CardClickHandler so drags and clicks/long-press don't fight.
    /// </summary>
    public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Runtime refs")]
        public CanvasGroup _canvasGroup;                // Used to control raycast blocking during drag

        private RectTransform _rectTransform;           // Transform for UI positioning
        private Vector3 _originalPosition;              // Position to return to if drop is invalid
        private Vector3 _originalScale;                 // Scale to restore
        private Quaternion _originalRotation;           // Rotation to restore after invalid drag
        private Canvas _canvas;                         // Root canvas (for scale factor)
        private CardClickHandler _click;                // Sibling click/press handler
        private CardObjectData _cardObjectData;

        [Header("Drag visuals")]
        [SerializeField] private float _growthFactor = 1.1f;
        [SerializeField] private float _growthDuration = 0.15f;

        public PlayerController Owner { get; set; }     // To set owner (if needed externally)

        private void Awake()
        {
            // Get required components for drag behavior
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
            _canvas = FindAnyObjectByType<Canvas>();
            _click = GetComponent<CardClickHandler>(); // May be null if not present
           _cardObjectData = GetComponent<CardObjectData>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _click?.SetDragging(true);

            // Store initial transform state
            _originalPosition = _rectTransform.position;
            _originalRotation = _rectTransform.rotation;
            _originalScale = _rectTransform.localScale;
            _cardObjectData.auraSelect.gameObject.SetActive(true);
            

            // Visual grow-on-pickup
            LMotion.Create(_originalScale, _originalScale * _growthFactor, _growthDuration)
                   .BindToLocalScale(transform);

            // Disable raycasts to allow card to pass through UI drop targets
            _canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Move in local (anchored) space by the pointer delta, compensating for canvas scale
            _rectTransform.anchoredPosition += eventData.delta / (_canvas ? _canvas.scaleFactor : 1f);
            _rectTransform.rotation = Quaternion.identity;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = true;
            _click?.SetDragging(false);

            // Vérifie d'abord s'il y a un objet sous le pointeur
            if (eventData.pointerEnter == null)
            {
                RevertPosition();
                return;
            }

            // Vérifie si c'est bien une zone de drop
            if (!eventData.pointerEnter.TryGetComponent(out CardDropZone zone))
            {
                RevertPosition();
                return;
            }

            // Vérifie si la zone est active
            if (!zone.enabled)
            {
                RevertPosition();
                return;
            }

            // --- VALID DROP ---
            _rectTransform.SetParent(zone.transform, worldPositionStays: false);
            _rectTransform.localRotation = Quaternion.identity;
            _rectTransform.localScale = _originalScale;
            _rectTransform.anchoredPosition = Vector2.zero;
        }


        public void RevertPosition()
        {
            // Re-enable raycasts for interaction detection (already true in OnEndDrag, but safe here too)
            _canvasGroup.blocksRaycasts = true;

            // Restore transform
            _rectTransform.position = _originalPosition;
            _rectTransform.rotation = _originalRotation;
            _cardObjectData.auraSelect.gameObject.SetActive(false);

            LMotion.Create(_rectTransform.localScale, _originalScale, _growthDuration)
                   .BindToLocalScale(transform);
        }
    }
}
