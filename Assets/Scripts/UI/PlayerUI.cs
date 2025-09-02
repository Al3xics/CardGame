using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
using Unity.Netcode;
using System.Linq;
using UnityEngine.Rendering;
using Cysharp.Threading.Tasks;
using static UnityEngine.UI.GridLayoutGroup;
using UnityEditor.SceneManagement;

namespace Wendogo
{
    public class PlayerUI : SerializedMonoBehaviour
    {
        private TextMeshProUGUI playerTitle;
        private TextMeshProUGUI readyText;
        private Button readyButton;
        private TextMeshProUGUI endText;
        private TextMeshProUGUI roleText;

        [SerializeField] private TextMeshProUGUI foodCount;
        [SerializeField] private TextMeshProUGUI woodCount;
        [SerializeField] private TextMeshProUGUI paCount;
        [SerializeField] private TextMeshProUGUI timerCount;
        [SerializeField] private TextMeshProUGUI playerName;

        private readonly HashSet<ulong> _subscribedPlayers = new HashSet<ulong>();

        [SerializeField] public List<GameObject> hearts = new List<GameObject>();

        public Dictionary<GameObject, ulong> UIPlayerID = new Dictionary<GameObject, ulong>();
        public Dictionary<Transform, GameObject> CardSpaces = new Dictionary<Transform, GameObject>();

        [OdinSerialize]
        private Dictionary<Image, Sprite> WendogoUI = new();


        [SerializeField] private RitualUI _ritualObject;
        private int _lastRitualStage = -1;

        public static PlayerUI Instance { get; private set; }



        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            playerTitle = transform.Find("PlayerTitle")?.GetComponent<TextMeshProUGUI>();
            readyText = transform.Find("ReadyText")?.GetComponent<TextMeshProUGUI>();
            readyButton = transform.Find("ReadyButton")?.GetComponent<Button>();
            endText = transform.Find("END")?.GetComponent<TextMeshProUGUI>();
            roleText = transform.Find("PlayerRole")?.GetComponent<TextMeshProUGUI>();

            if (readyText != null) readyText.gameObject.SetActive(false);
            if (endText != null) endText.gameObject.SetActive(false);
        }



        public void DefineFoodText(int foodAmount, ulong target = 0)
        {
            if (target == 0)
                foodCount.text = $"{foodAmount.ToString()}";
        }
        public void DefineWoodText(int woodAmount, ulong target = 0)
        {
            if (target == 0)
                woodCount.text = $"{woodAmount.ToString()}";
        }

        public void DefinePAs(int paAmount)
        {
            paCount.text = $"{paAmount.ToString()}/2";
        }

        public void DefineTimer(int timeLeft)
        {
            timerCount.text = $"{timeLeft.ToString()}";
        }

        public void SendDebug(string message)
        { Debug.Log(message); }

        public void RenamePlayer(ulong playerID)
        {
            var player = PlayerController.GetPlayer(playerID);
            if (AutoSessionBootstrapper.AutoConnect)
                playerName.text = player.name;
            else
            {
                playerName.text = SessionManager.Instance.ActiveSession.CurrentPlayer.Properties[SessionConstants.PlayerNamePropertyKey].Value.ToString();
            }
        }

        public void GetRole(string role)
        {
            if (roleText != null)
                roleText.text = role;
        }

        public void EndValidation()
        {
            if (endText != null)
                endText.gameObject.SetActive(true);
        }

        public void SetWendogoUI()
        {
            foreach (var visual in WendogoUI)
            {
                visual.Key.sprite = visual.Value;
            }
        }

        public void SetUIInfos(ulong localPlayerID)
        {
            SetRitualUI();
            RenamePlayer(localPlayerID);
            
            List<ulong> allPlayerIds = NetworkManager.Singleton.ConnectedClientsList.Select(x => x.ClientId).ToList(); // List of all connected players
            allPlayerIds.Remove(localPlayerID); // Remove local player
            GameObject[] slots = UIPlayerID.Keys.ToArray();

            for (int i = 0; i < slots.Length; i++)
            {
                GameObject go = slots[i];
                ulong trueID = allPlayerIds[i];
                
                var player = PlayerController.GetPlayer(trueID);
                if (player == null)
                    continue;

                PlayerController.PlayerSlots[trueID] = i + 1;
                OtherPlayerUIContent otherUI = go.GetComponent<OtherPlayerUIContent>();

                if (!_subscribedPlayers.Contains(trueID))
                {
                    GameObject slot = go;

                    player.wood.OnValueChanged += (oldVal, newVal) =>
                    {
                        var txt = otherUI.woodUI;
                        txt.text = newVal.ToString();
                    };
                    player.food.OnValueChanged += (oldVal, newVal) =>
                    {
                        var txt = otherUI.foodUI;
                        txt.text = newVal.ToString();
                    };
                    player.health.OnValueChanged += (oldVal, newVal) =>
                    {
                        if (newVal < oldVal)
                            for (int i = newVal; i < oldVal; i++)
                            {
                                otherUI.hearts[i].gameObject.SetActive(false);
                            }
                        else if (newVal > oldVal)
                            for (int i = oldVal; i < newVal; i++)
                            {
                                otherUI.hearts[i].gameObject.SetActive(true);
                            }
                    };

                    int lastPassiveCount = player.PassiveCards.Count;
                    player.PassiveCards.OnListChanged += (Unity.Netcode.NetworkListEvent<int> _) =>
                    {
                        int newCount = player.PassiveCards.Count;

                        if (newCount < lastPassiveCount)
                        {
                            for (int i = newCount; i < lastPassiveCount; i++)
                                otherUI.passiveCards[i].gameObject.SetActive(false);
                        }
                        else if (newCount > lastPassiveCount)
                        {
                            for (int i = lastPassiveCount; i < newCount; i++)
                                otherUI.passiveCards[i].gameObject.SetActive(true);
                        }

                        lastPassiveCount = newCount;
                    };

                    _subscribedPlayers.Add(trueID);
                }

                var title = go.GetComponentInChildren<TextMeshProUGUI>();
                if (AutoSessionBootstrapper.AutoConnect)
                {
                    title.text = player.name;
                }
                else
                {
                    title.text = ServerManager.GlobalPlayersByName.TryGetValue(trueID, out var name) ? name : $"Sah Player {trueID}";
                }

            }
        }

        private void SetRitualUI()
        {
            TextMeshProUGUI ritualWood = _ritualObject.woodUI;
            ritualWood.text = $"{ServerManager.Instance._woodInRitual.Value}";

            ServerManager.Instance._woodInRitual.OnValueChanged += (oldVal, newVal) =>
            {
                ritualWood.text = $"{newVal}";

                if (newVal < oldVal)
                    for (int i = newVal; i < oldVal; i++)
                    {
                        _ritualObject.woodGaugeParts[i].gameObject.SetActive(false);
                    }
                else if (newVal > oldVal)
                    for (int i = oldVal; i < newVal; i++)
                    {
                        _ritualObject.woodGaugeParts[i].gameObject.SetActive(true);
                    }
                UpdateRitualParts();
            };

            TextMeshProUGUI ritualFood = _ritualObject.foodUI;
            List<GameObject> foodGauge = _ritualObject.foodGaugeParts;
            ritualFood.text = ServerManager.Instance._foodInRitual.Value.ToString() ;
            ServerManager.Instance._foodInRitual.OnValueChanged += (oldVal, newVal) =>
            {
                ritualFood.text = $"{newVal}";

                if (newVal < oldVal)
                    for (int i = newVal; i < oldVal; i++)
                    {
                        _ritualObject.foodGaugeParts[i].gameObject.SetActive(false);
                    }
                else if (newVal > oldVal)
                    for (int i = oldVal; i < newVal; i++)
                    {
                        _ritualObject.foodGaugeParts[i].gameObject.SetActive(true);
                    }
                UpdateRitualParts();
            };
            UpdateRitualParts();
        }

        private void UpdateRitualParts()
        {
            const int MaxTotal = 12;
            const int OneThird = MaxTotal / 3;
            const int TwoThirds = 2 * MaxTotal / 3;
            int wood = ServerManager.Instance._woodInRitual.Value;
            int food = ServerManager.Instance._foodInRitual.Value;
            int total = Mathf.Clamp(wood + food, 0, MaxTotal);

            foreach (var part in _ritualObject.ritualParts)
                part.gameObject.SetActive(false);

            int stage = -1;
            if (total >= MaxTotal) stage = 2;
            else if (total >= TwoThirds) stage = 1;
            else if (total >= OneThird) stage = 0;

            for (int i = 0; i < _ritualObject.ritualParts.Count; i++)
                _ritualObject.ritualParts[i].SetActive(i <= stage && stage >= 0);

            int halfTotal = MaxTotal / 2;
            if (wood == halfTotal)
            {
                ServerManager.Instance.BroadcastSharedFXEventRpc(new FXEventContext
                {
                    fxType = FXEventType.OnRitualWoodComplete,
                    playerID = PlayerController.LocalPlayer.LocalPlayerId
                });
            }

            if (food == halfTotal)
            {
                ServerManager.Instance.BroadcastSharedFXEventRpc(new FXEventContext
                {
                    fxType = FXEventType.OnRitualFoodComplete,
                    playerID = PlayerController.LocalPlayer.LocalPlayerId
                });
            }

            if (stage > _lastRitualStage)
            {
                if (stage == 0)
                {
                    ServerManager.Instance.BroadcastSharedFXEventRpc(new FXEventContext
                    {
                        fxType = FXEventType.OnRitual1,
                        playerID = PlayerController.LocalPlayer.LocalPlayerId
                    });
                }
                else if (stage == 1)
                {
                    ServerManager.Instance.BroadcastSharedFXEventRpc(new FXEventContext
                    {
                        fxType = FXEventType.OnRitual2,
                        playerID = PlayerController.LocalPlayer.LocalPlayerId
                    });
                }
                else if (stage == 2)
                {
                    ServerManager.Instance.BroadcastSharedFXEventRpc(new FXEventContext
                    {
                        fxType = FXEventType.OnRitual3,
                        playerID = PlayerController.LocalPlayer.LocalPlayerId
                    });
                }
            }
            _lastRitualStage = stage;
        }

        public void DeactivateAllHearts()
        {
            foreach (var heart in hearts)
            {
                if (heart.activeSelf)
                    heart.gameObject.SetActive(false);
            }
        }

    }
}