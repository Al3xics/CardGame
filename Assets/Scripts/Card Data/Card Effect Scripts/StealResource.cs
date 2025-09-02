using TMPro;
using Unity.Services.Analytics;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "StealResource", menuName = "Card Effects/Steal Resource")]
    public class StealResource : CardEffect
    {
        public int ResourceAmount = 1;
        public GameObject prefabUITarget;
        private GameObject _uiStealRessourceInstance;

        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            Debug.Log($"Stealing resource from {target} by {owner}.");

            PlayerController ownerPlayer = PlayerController.GetPlayer(owner);
            PlayerController targetPlayer = PlayerController.GetPlayer(target);
            bool isNight = targetPlayer.IsSimulatingNight;

            if (value != 1000)
            {
                //temp
                //Change you can pick the resource
                //value = Random.Range(0, 2);
                if (value == 0) // steal wood
                {
                    if (targetPlayer.wood.Value <= 0) return;

                    if (!isNight)
                    {
                        ServerManager.Instance.BroadcastLocalFXEventToPlayerRpc(new FXEventContext
                        {
                            fxType = FXEventType.OnStolenWood,
                            playerID = target
                        });
                    }
                    
                    ServerManager.Instance.BroadcastLocalFXEventToPlayerRpc(new FXEventContext
                    {
                        fxType = FXEventType.OnStealWood,
                        playerID = owner
                    });

                    ServerManager.Instance.ChangePlayerResourceRpc(value, -ResourceAmount, target);
                    ServerManager.Instance.ChangePlayerResourceRpc(value, ResourceAmount, owner);
                }
                else if (value == 1) // steal food
                {
                    if (targetPlayer.food.Value <= 0) return;

                    if (!isNight)
                    {
                        ServerManager.Instance.BroadcastLocalFXEventToPlayerRpc(new FXEventContext
                        {
                            fxType = FXEventType.OnStolenFood,
                            playerID = target
                        });
                    }
                    
                    ServerManager.Instance.BroadcastLocalFXEventToPlayerRpc(new FXEventContext
                    {
                        fxType = FXEventType.OnStealFood,
                        playerID = owner
                    });

                    ServerManager.Instance.ChangePlayerResourceRpc(value, -ResourceAmount, target);
                    ServerManager.Instance.ChangePlayerResourceRpc(value, ResourceAmount, owner);
                }
                AnalyticsManager.Instance.RecordEvent(new CustomEvent("stealResourceActiveCardWasApplied"));
            }
        }


        public override void ShowUI(GameObject uiInstance = null)
        {
            if (_uiStealRessourceInstance == null)
                _uiStealRessourceInstance = Instantiate(prefabUITarget);
            base.ShowUI(_uiStealRessourceInstance);
            prefabUITarget.SetActive(true);
            
            ServerManager.GlobalPlayersByName.TryGetValue(0, out var value1);
            ServerManager.GlobalPlayersByName.TryGetValue(1, out var value2);
            ServerManager.GlobalPlayersByName.TryGetValue(2, out var value3);
            ServerManager.GlobalPlayersByName.TryGetValue(3, out var value4);
            prefabUITarget.transform.Find("Panel").transform.Find("ButtonPlayer1").transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = value1;
            prefabUITarget.transform.Find("Panel").transform.Find("ButtonPlayer2").transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = value2;
            prefabUITarget.transform.Find("Panel").transform.Find("ButtonPlayer3").transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = value3;
            prefabUITarget.transform.Find("Panel").transform.Find("ButtonPlayer4").transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = value4;
        }

        public override void HideUI(bool clearVote, GameObject uiInstance = null)
        {
            _uiStealRessourceInstance.SetActive(false);
            Destroy(_uiStealRessourceInstance.gameObject);
            base.HideUI(clearVote, _uiStealRessourceInstance);
        }

    }
}

