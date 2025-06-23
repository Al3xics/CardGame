using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "CardType", menuName = "Scriptable Objects/CardType")]
public class CardTypeSO : SerializedScriptableObject
{
    public CardEffect[] Effects;
}
public abstract class CardEffect
{
    public virtual void Apply(ulong owner, ulong target, int value = default)
    {

    }
}
