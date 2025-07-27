using Unity.Services.Analytics;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "FightOrFlight", menuName = "Card Effects/Fight Or Flight")]
    public class FightOrFlight : CardEffect
    {
        public GameObject prefabUI;
        public int damageDone = 1;
        
        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            PlayerController playerOwner = PlayerController.GetPlayer(owner);
            PlayerController playerTarget = PlayerController.GetPlayer(target);
            
            if (value <= -1) value = 0;
            var newValue = damageDone + value;

            if (playerOwner.Role.Value == RoleType.Wendogo)
            {
                if (!playerTarget.hasGuardian.Value && !playerTarget.isFlighting)
                {
                    playerTarget.ChangeHealth(newValue);
                }
                else
                {
                    playerTarget.guardian.ChangeHealth(newValue);
                    playerTarget.hasGuardian.Value = false;
                }
            }
            else
            {
                playerOwner.isFlighting = true;
            }
            
            AnalyticsManager.Instance.RecordEvent(new CustomEvent("fightOrFlightActiveCardWasApplied"));
        }

        public void ShouldUsUI(ulong owner, ulong target, out int value)
        {
            PlayerController playerOwner = PlayerController.GetPlayer(owner);
            PlayerController playerTarget = PlayerController.GetPlayer(target);

            if (playerOwner.Role.Value == RoleType.Wendogo)
            {
                value = 0;
            }
            else
            {
                value = 1;
            }
        }
        
        public override void ShowUI()
        {
            if (prefabUI == null)
                prefabUI = FindAnyObjectByType<CanvaTarget>(FindObjectsInactive.Include).gameObject;
            prefabUI.SetActive(true);
        }

        public override void HideUI()
        {
            if (prefabUI == null)
                prefabUI = FindAnyObjectByType<CanvaTarget>(FindObjectsInactive.Include).gameObject;
            prefabUI.SetActive(false);
        }
    }
}
