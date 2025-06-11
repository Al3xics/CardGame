using UnityEngine;
using UnityEngine.EventSystems;

namespace Wendogo
{

    public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;

        public PlayerController Owner { get; set; }

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _originalPosition = _rectTransform.position;
            _originalRotation = _rectTransform.rotation;
            _canvasGroup.blocksRaycasts = false; 
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.position = eventData.position;
            _rectTransform.rotation = Quaternion.identity;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = true;

            if (eventData.pointerEnter == null || !eventData.pointerEnter.TryGetComponent(out CardDropZone zone))
            {
                _rectTransform.position = _originalPosition; 
                _rectTransform.rotation = _originalRotation; 
            }
        }
    }

}