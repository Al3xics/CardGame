using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using System.Linq;

namespace Wendogo
{
    //Defines a valid drop target for draggable card objects
    public class CardDropZone : SerializedMonoBehaviour, IDropHandler
    {
        #region Variables

        public static event Action<CardDataSO> OnCardDataDropped;
        public static event Action<CardObjectData> OnCardDropped;

        public bool isOccupied;

        #endregion


        public void OnDrop(PointerEventData eventData)
        {
            var draggedCard = eventData.pointerDrag;
            if (draggedCard == null)
                return;

            if (!draggedCard.TryGetComponent<CardDragHandler>(out var dragHandler) ||
                !draggedCard.TryGetComponent<CardObjectData>(out var cod))
                return;

            var cardData = cod.Card;
            if (cardData.isPassive)
            {
                var handManager = FindFirstObjectByType<HandManager>();
                handManager?.AddCardToPassiveZone(draggedCard);

                var slots = PlayerUI.Instance.CardSpaces;
                foreach (Transform zone in slots.Keys.ToList())  
                {
                    if (slots[zone] == null)
                    {
                        AnimateCardToZone(draggedCard.transform, zone);

                        slots[zone] = draggedCard;
                        break;
                    }
                }
            }

            dragHandler.enabled = false;
            OnCardDataDropped?.Invoke(cardData);
            OnCardDropped?.Invoke(cod);
        }

        /// <summary>
        /// Tweening to move the cards to the zone position
        /// </summary>
        private void AnimateCardToZone(Transform card, Transform zone)
        {
            LMotion.Create(card.position, zone.position, 0.2f)
                   .WithEase(Ease.OutQuad)
                   .BindToPosition(card);

            LMotion.Create(card.localScale, zone.localScale / 2.5f, 0.2f)
                   .WithEase(Ease.OutQuad)
                   .BindToLocalScale(card);

            LMotion.Create(card.rotation, zone.rotation, 0.2f)
                   .WithEase(Ease.OutQuad)
                   .BindToRotation(card);
        }
    }
}
