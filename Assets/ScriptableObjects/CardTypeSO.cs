using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "CardType", menuName = "Scriptable Objects/CardType")]
public class CardTypeSO : ScriptableObject
{
    //Effect variable
    //Method for applying the effect
    [OdinSerialize]
    public CardEffect[] effects;
}
