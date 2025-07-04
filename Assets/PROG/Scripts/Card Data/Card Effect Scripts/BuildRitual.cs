using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "BuildRitual", menuName = "Card Effects/Build Ritual")]
    public class BuildRitual : CardEffect
    {
        public GameObject selectResourcePrefab;

        public void ApplyRitualEffect(ulong origin, List<int> resourceList)
        {
            var player = PlayerController.GetPlayer(origin);
            if (resourceList.Count > 0)
            {
                foreach (var resource in resourceList)
                {
                    if (resource == 0 && player.IsSimulatingNight)
                    {
                        player.hiddenFood -= 1;
                        if (player.Role.Value is RoleType.Wendogo)
                        {
                            //add un false dans la liste cachée food
                        }
                        else
                        {
                            //add un true dans la liste cachée food
                        }
                    } 
                    else if (resource == 0 && !player.IsSimulatingNight)
                    {
                        player.food.Value -= 1;
                        if (player.Role.Value is RoleType.Wendogo)
                        {
                            //add un false dans la liste food
                        }
                        else
                        {
                            //add un true dans la liste food
                        }
                    }
                    else if (resource == 1 && player.IsSimulatingNight)
                    {
                        player.hiddenWood -= 1;
                        if (player.Role.Value is RoleType.Wendogo)
                        {
                            //add un false dans la liste cachée wood
                        }
                        else
                        {
                            //add un true dans la liste cachée wood
                        }
                    }
                    else if (resource == 1 && !player.IsSimulatingNight)
                    {
                        player.wood.Value -= 1;
                        if (player.Role.Value is RoleType.Wendogo)
                        {
                            //add un false dans la liste wood
                        }
                        else
                        {
                            //add un true dans la liste wood
                        }
                    }
                }
            }
        }
        
        public override void ShowUI()
        {
            selectResourcePrefab.SetActive(true);
        }
        
        public override void HideUI()
        {
            selectResourcePrefab.SetActive(false);
        }
    }
}
