using UnityEngine;
using LitMotion.Extensions;
using LitMotion;
using Unity.VisualScripting;

//Manager general of tweens
public class TweeningManager : MonoBehaviour
{
    //Tween animation that makes the card gor up to show that the card is being interacted with
    public static void CardUp(Transform cardTransform)
    {
        LMotion.Create(cardTransform.position.y, cardTransform.position.y +25, 0.2f)
               .WithEase(Ease.OutQuad)
               .BindToPositionY(cardTransform);
    }

    //Tween animation that makes the card go down to show that we're done focusing with the card
    public static void CardDown(Transform cardTransform)
    {
        LMotion.Create(cardTransform.position.y, cardTransform.position.y - 25, 0.2f)
               .WithEase(Ease.OutQuad)
               .BindToPositionY(cardTransform);
    }
}
