using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Wendogo
{
    // Handles drag-and-drop behavior for card objects in the UI
    public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private CanvasGroup _canvasGroup;           //Used to control raycast blocking during drag
        private RectTransform _rectTransform;       //Transform for UI positioning
        private Vector3 _originalPosition;          //Position to return to if drop is invalid
        private Quaternion _originalRotation;       //Rotation to restore after invalid drag

        public PlayerController Owner { get; set; } //To set owner

        private void Awake()
        {
            //Get required components for drag behavior
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            //Store initial position and rotation in case drag is cancelled or invalid
            _originalPosition = _rectTransform.position;
            _originalRotation = _rectTransform.rotation;

            //Disable raycasts to allow card to pass through UI drop targets
            _canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            //Move card slide and reset rotation to upright
            _rectTransform.position = eventData.position;
            _rectTransform.rotation = Quaternion.identity;
        }

        public void OnEndDrag(PointerEventData eventData)
        {

            _canvasGroup.blocksRaycasts = true;

            //Check if the card was dropped on a valid drop zone
            if (eventData.pointerEnter == null || !eventData.pointerEnter.TryGetComponent(out CardDropZone zone))
            {
                RevertPosition();
            }

        }
        public void RevertPosition()
        {
            //Re-enable raycasts for interaction detection
            _canvasGroup.blocksRaycasts = true;
            _rectTransform.position = _originalPosition;
            _rectTransform.rotation = _originalRotation;
        }

        public static void ToggleMovingCards(List<GameObject> cardsInHand)
        {
            foreach (GameObject card in cardsInHand)
            {
                CardDragHandler handler = card.GetComponent<CardDragHandler>();
                handler.enabled = !handler.enabled;
            }
        }

    }
}
