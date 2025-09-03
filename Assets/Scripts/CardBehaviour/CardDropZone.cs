using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using System.Linq;
using UnityEditor.Rendering;

namespace Wendogo
{
    //Defines a valid drop target for draggable card objects
    public class CardDropZone : SerializedMonoBehaviour, IDropHandler
    {
        #region Variables

        public static event Action<CardDataSO> OnCardDataDropped;
        public static event Action<CardObjectData> OnCardDropped;
        public static event Action<CardObjectData> OnCardBurned;

        [SerializeField] private bool isBurning;

        [SerializeField] private int burnCount;

        #endregion

        private void OnEnable()
        {
            burnCount = 0;
        }

        public void OnDrop(PointerEventData eventData)
        {
            var draggedCard = eventData.pointerDrag;
            if (draggedCard == null)
                return;

            if (!draggedCard.TryGetComponent<CardDragHandler>(out var dragHandler) ||
                !draggedCard.TryGetComponent<CardObjectData>(out var cod))
                return;

            if (isBurning)
            {
                burnCount++;
                OnCardBurned?.Invoke(cod);
                if (burnCount >= 2)
                {
                    enabled = false;
                }
                return;
            }

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
            else
            {
                LMotion.Create(draggedCard.transform.position, gameObject.transform.position, 0.3f)
                    .WithEase(Ease.OutQuad)
                    .BindToPosition(draggedCard.transform);

                LMotion.Create(draggedCard.transform.localScale, Vector3.zero, 0.5f)
                    .WithEase(Ease.OutQuad)
                    .WithOnComplete(() =>
                    {
                        dragHandler.enabled = false;
                        CallZoneDropEvents(cod, cardData);
                    })
                    .BindToLocalScale(draggedCard.transform);
                return;
            }


            dragHandler.enabled = false;
            CallZoneDropEvents(cod, cardData);
            if (cardData.CardEffect is Sacrifice)
            {
                LMotion.Create(cod.gameObject.transform.localScale, new Vector3(12, 12, 12) / 2.5f, 0.2f)
                    .WithEase(Ease.OutQuad)
                    .BindToLocalScale(cod.gameObject.transform);
            }
        }

        public static void CallZoneDropEvents(CardObjectData cod, CardDataSO cardData)
        {
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

            LMotion.Create(card.localScale, new Vector3(12, 12, 12) / 2.5f, 0.2f)
                   .WithEase(Ease.OutQuad)
                   .BindToLocalScale(card);

            LMotion.Create(card.rotation, Quaternion.Euler(0, 0, -90), 0.2f)
                   .WithEase(Ease.OutQuad)
                   .BindToRotation(card);
        }
    }
}
