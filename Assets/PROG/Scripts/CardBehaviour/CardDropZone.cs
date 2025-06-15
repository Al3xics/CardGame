using UnityEngine;
using UnityEngine.EventSystems;
using Wendogo;

//Defines a valid drop target for draggable card objects
public class CardDropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        //Get the object currently being dragged
        GameObject draggedCard = eventData.pointerDrag;

        //Check if the dragged object is valid and has a CardDragHandler component
        if (draggedCard != null && draggedCard.TryGetComponent(out CardDragHandler card))
        {
            //Snap the dragged card to this drop zone's position and rotation
            draggedCard.transform.SetPositionAndRotation(transform.position, transform.rotation);
        }
    }
}
