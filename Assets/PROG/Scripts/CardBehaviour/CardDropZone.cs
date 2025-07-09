using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;

namespace Wendogo
{
    //Defines a valid drop target for draggable card objects
    public class CardDropZone : SerializedMonoBehaviour, IDropHandler
    {
        #region Variables

        public static event Action<CardDataSO> OnCardDataDropped;
        public static event Action<CardObjectData> OnCardDropped;

        public bool isOccupied;

        enum ZoneType
        {
            Active,
            Passive
        }

        [SerializeField, EnumToggleButtons]
        private ZoneType zoneType = ZoneType.Active;

        #endregion

        public async void OnDrop(PointerEventData eventData)
        {
            //Get the object currently being dragged
            GameObject draggedCard = eventData.pointerDrag;

            //Check if the dragged object is valid and has a CardDragHandler component
            if (draggedCard != null && draggedCard.TryGetComponent(out CardDragHandler card))
            {
                CardObjectData cod = draggedCard.GetComponent<CardObjectData>();
                CardDataSO cardData = cod.Card;

                //bool isActiveDrop = zoneType == ZoneType.Active && !cardData.isPassive;
                //bool isPassiveDrop = zoneType == ZoneType.Passive && cardData.isPassive;
                //if (!(isActiveDrop || isPassiveDrop))
                //{
                //    card.RevertPosition(); ;
                //    return;  
                //}

                if (cardData.isPassive && zoneType == ZoneType.Passive)
                {
                    var handManager = FindFirstObjectByType<HandManager>();
                    handManager?.AddCardToPassiveZone(draggedCard);

                    foreach (var zone in PlayerUI.Instance.cardSpaces)
                    {
                        LMotion.Create(
                                draggedCard.transform.position,
                                zone.transform.position,
                                0.2f
                            )
                            .WithEase(Ease.OutQuad)
                            .BindToPosition(draggedCard.transform);

                        //draggedCard.transform.SetPositionAndRotation(zone.gameObject.transform.position, zone.gameObject.transform.rotation);
                        PlayerUI.Instance.cardSpaces.Remove(zone);
                        break;

                    }

                }

                draggedCard.transform.SetPositionAndRotation(transform.position, transform.rotation);
                card.enabled = false;
                OnCardDataDropped?.Invoke(cardData);
                OnCardDropped?.Invoke(cod);

                //if (isActiveDrop)
                //{
                //    await UniTask.WaitForSeconds(2);
                //    Destroy(cod.gameObject);
                //}
            }
        }
    }
}
