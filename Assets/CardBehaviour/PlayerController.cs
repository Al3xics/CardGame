using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static CardObjectData ActiveCard;

    public void EnableInput()
    {
        //Implement enable input
        Debug.Log("Input enabled");
    }

    public void SelectCard(bool cardHasTarget = false)
    {
        //Implement select card
        if (cardHasTarget)
            SelectTarget();

        TweeningManager.CardUp(ActiveCard.gameObject.transform);
        ActiveCard.isSelected = true;

        Debug.Log("Card selected");
    }    
    
    public void DeselectCard()
    {
        //Implement select card
        Debug.Log("Card deselected");

        TweeningManager.CardDown(ActiveCard.gameObject.transform);
        ActiveCard.isSelected = false;
    }

    public void SelectTarget()
    {
        //Implement select target
        Debug.Log("Target selected");
    }

    public void ConfirmPlay()
    {
        //Implement confirm play
        //Check player PA
        Debug.Log("Confirm play");
    }

}
