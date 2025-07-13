using Sirenix.OdinInspector;
using UnityEngine;

namespace Wendogo
{
    [HideMonoScript]
    public abstract class CardEffect : ScriptableObject
    {
        //public virtual event Action OnTargetDetection;
        public virtual void Apply(ulong owner, ulong target, int value = -1) { }

        /// <summary>
        /// Use passive cards.
        /// </summary>
        /// <param name="playedCardId">The card ID. You need to get the <see cref="CardDataSO.CardEffect"/> from the <see cref="CardDataSO"/> script to use it.</param>
        /// <param name="origin">The ID of the player using the card.</param>
        /// <param name="target">The targeted ID player.</param>
        /// <param name="value">The bonus value. <c>-1</c> if the value is null.</param>
        /// <returns></returns>
        public virtual bool ApplyPassive(int playedCardId, ulong origin, ulong target, out int value)
        {
            value = -1;
            return false;
        }

        public virtual void ShowUI() {}
        
        public virtual void HideUI() {}
    }
}
