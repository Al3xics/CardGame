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
        public bool isDecoy;

        public void ApplyRitualEffect(ulong owner, ResourceType resourceType, int value)
        {
            if (value <= 0)
            {
                return;
            }


            var player = PlayerController.GetPlayer(owner);
            if (!player) return;

            bool isNight = player.IsSimulatingNight;
            bool isFood = resourceType == ResourceType.Food;
            
            AnalyticsManager.Instance.RecordEvent(new CustomEvent("buildRitualActiveCardWasApplied"));

            if (isFood)
            {
                // Handle food
                if (isNight)
                    player.hiddenFood = player.hiddenFood - value;
                else
                {
                    player.food.Value = player.food.Value - value;
                
                    ServerManager.Instance.BroadcastSharedFXEventRpc(new FXEventContext
                    {
                        fxType = FXEventType.OnBuildRitualFood,
                        playerID = owner
                    });
                }

                ServerManager.Instance.AddRessourceToRitualRpc(isNight, true, !isDecoy, value);
            }
            else
            {
                // Handle wood
                if (isNight)
                    player.hiddenWood = player.hiddenWood - value;
                else
                {
                    player.wood.Value = player.wood.Value - value;

                    ServerManager.Instance.BroadcastSharedFXEventRpc(new FXEventContext
                    {
                        fxType = FXEventType.OnBuildRitualWood,
                        playerID = owner
                    });
                }
                
                ServerManager.Instance.AddRessourceToRitualRpc(isNight, false, !isDecoy, value);
            }
        }

        public override void ShowUI(GameObject uiInstance = null)
        {
            if (_ressouceCanvaInstance == null)
                _ressouceCanvaInstance = Instantiate(SelectResourcePrefab);
            base.ShowUI(_ressouceCanvaInstance);
            _ressouceCanvaInstance.SetActive(true);
        }

        public override void HideUI(bool clearVotes, GameObject uiInstance = null)
        {
            _ressouceCanvaInstance.SetActive(false);
            Destroy( _ressouceCanvaInstance );
            base.HideUI(clearVotes, _ressouceCanvaInstance);
        }
    }
}
