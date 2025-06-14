using UnityEngine;
using UnityEngine.EventSystems;
using Wendogo;

public class CardDropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedCard = eventData.pointerDrag;

        if (draggedCard != null && draggedCard.TryGetComponent(out CardDragHandler card))
        {
            draggedCard.transform.SetPositionAndRotation(transform.position, transform.rotation);
        }
    }
}
