using System.Collections.Generic;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "BuildRitual", menuName = "Card Effects/Build Ritual")]
    public class BuildRitual : CardEffect
    {
        public GameObject SelectResourcePrefab;
        public int RitualCost = 1;

        public void ApplyRitualEffect(ulong origin, List<int> resourceList)
        {
            var player = PlayerController.GetPlayer(origin);
            if (resourceList.Count > 0)
            {
                foreach (var resource in resourceList)
                {
                    if (resource == 0 && player.IsSimulatingNight)
                    {
                        player.hiddenFood -= RitualCost;
                        if (player.Role.Value is RoleType.Wendogo)
                            //add un false dans la liste cachée food
                            ServerManager.Instance.AddRessourceToRitualRpc(true, true, false);
                        else
                            //add un true dans la liste cachée food
                            ServerManager.Instance.AddRessourceToRitualRpc(true, true, true);
                    } 
                    else if (resource == 0 && !player.IsSimulatingNight)
                    {
                        player.food.Value -= RitualCost;
                        if (player.Role.Value is RoleType.Wendogo)
                            //add un false dans la liste food
                            ServerManager.Instance.AddRessourceToRitualRpc(false, true, false);
                        else
                            //add un true dans la liste food
                            ServerManager.Instance.AddRessourceToRitualRpc(false, true, true);
                    }
                    else if (resource == 1 && player.IsSimulatingNight)
                    {
                        player.hiddenWood -= RitualCost;
                        if (player.Role.Value is RoleType.Wendogo)
                            //add un false dans la liste cachée wood
                            ServerManager.Instance.AddRessourceToRitualRpc(true, false, false);
                        else
                            //add un true dans la liste cachée wood
                            ServerManager.Instance.AddRessourceToRitualRpc(true, false, true);
                    }
                    else if (resource == 1 && !player.IsSimulatingNight)
                    {
                        player.wood.Value -= RitualCost;
                        if (player.Role.Value is RoleType.Wendogo)
                            //add un false dans la liste wood
                            ServerManager.Instance.AddRessourceToRitualRpc(false, false, false);
                        else
                            //add un true dans la liste wood
                            ServerManager.Instance.AddRessourceToRitualRpc(false, false, true);
                    }
                }
            }
        }
        
        public override void ShowUI()
        {
            SelectResourcePrefab.SetActive(true);
        }
        
        public override void HideUI()
        {
            SelectResourcePrefab.SetActive(false);
        }
    }
}
