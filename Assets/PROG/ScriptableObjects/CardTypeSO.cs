using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "CardType", menuName = "Scriptable Objects/CardType")]
public class CardTypeSO : SerializedScriptableObject
{
    public CardEffect[] effects;
}
