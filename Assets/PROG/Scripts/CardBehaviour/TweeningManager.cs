using UnityEngine;
using LitMotion.Extensions;
using LitMotion;
using Unity.VisualScripting;

public class TweeningManager : MonoBehaviour
{
    public static void CardUp(Transform cardTransform)
    {
        LMotion.Create(cardTransform.position.y, cardTransform.position.y +25, 0.2f)
               .WithEase(Ease.OutQuad)
               .BindToPositionY(cardTransform);
    }

    public static void CardDown(Transform cardTransform)
    {
        LMotion.Create(cardTransform.position.y, cardTransform.position.y - 25, 0.2f)
               .WithEase(Ease.OutQuad)
               .BindToPositionY(cardTransform);
    }
}
