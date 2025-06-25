using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "Trap", menuName = "Card Effects/Trap")]
    public class Trap : CardEffect
    {
        public int defendValue = 5;

        public bool ApplyPassive(int attackingEffectId, ulong origin, ulong target, out int value)
        {
            DataCollection script = GameObject.Find("DataCollection").GetComponent<DataCollection>();
            //script.effects.TryGetValue(attackingEffectId, out CardEffect cardEffect);

            //if (cardEffect is GroupAttack)
            {
                //value = GroupAttack.AttackValue - defendValue;
                //return true;
            }

            value = -1;
            return false;
        }
    }
}
