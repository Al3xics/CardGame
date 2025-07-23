using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "BuildRitual", menuName = "Card Effects/Build Ritual")]
    public class BuildRitual : CardEffect
    {
        public GameObject SelectResourcePrefab;
        private GameObject _ressouceCanvaInstance;
        public int RitualCost = 1;

        public void ApplyRitualEffect(ulong owner, ResourceType resourceType, int value)
        {
            if (value <= 0) return;

            var player = PlayerController.GetPlayer(owner);
            if (!player) return;

            bool isNight = player.IsSimulatingNight;
            bool isWendogo = player.Role.Value is RoleType.Wendogo;
            bool isFood = resourceType == ResourceType.Food;
            
            AnalyticsManager.Instance.RecordEvent(new CustomEvent("buildRitualActiveCardWasApplied"));

            if (isFood)
            {
                // Handle food
                if (isNight)
                    player.hiddenFood -= value;
                else
                    player.food.Value -= value;

                ServerManager.Instance.AddRessourceToRitualRpc(isNight, true, !isWendogo, value);
            }
            else
            {
                // Handle wood
                if (isNight)
                    player.hiddenWood -= value;
                else
                    player.wood.Value -= value;

                ServerManager.Instance.AddRessourceToRitualRpc(isNight, false, !isWendogo, value);
            }
        }

        public override void ShowUI()
        {
            if (_ressouceCanvaInstance == null)
                _ressouceCanvaInstance = Instantiate(SelectResourcePrefab);
            _ressouceCanvaInstance.SetActive(true);
        }

        public override void HideUI()
        {
            _ressouceCanvaInstance.SetActive(false);
        }
    }
}
