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

    /// <summary>
    /// Use passive cards
    /// </summary>
    /// <param name="attackingEffect">The card effect.</param>
    /// <param name="origin">The ID of the player using the card.</param>
    /// <param name="target">The targeted ID player.</param>
    /// <param name="value">The bonus value. <c>-1</c> if the value is null.</param>
    /// <returns></returns>
    public virtual bool ApplyPassive(CardEffect attackingEffect, ulong origin, ulong target, out int value)
    {
        value = -1;
        return false;
    }
}
