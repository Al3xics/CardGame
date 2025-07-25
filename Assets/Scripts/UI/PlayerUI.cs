using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
using Unity.Netcode;
using System.Linq;

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

        private readonly HashSet<ulong> _subscribedPlayers = new HashSet<ulong>();

        [SerializeField] private TextMeshProUGUI foodCountP2;

        [SerializeField] public List<GameObject> hearts = new List<GameObject>();

        public Dictionary<GameObject, ulong> UIPlayerID = new Dictionary<GameObject, ulong>();
        public Dictionary<Transform, GameObject> CardSpaces = new Dictionary<Transform, GameObject>();

        [SerializeField] private RectTransform _ritualObject;

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

        public void SendDebug(string message)
        { Debug.Log(message); }

        public void RenamePlayer(string name)
        {
            if (playerTitle != null)
                playerTitle.text = name;
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

        [Rpc(SendTo.SpecifiedInParams)]
        public void SetUIInfos(ulong localPLayerID, RpcParams rpcParams)
        {
            SetRitualUI();

            //todo call the method in server manager in a loop for all players 
            //when the game starts
            KeyValuePair<GameObject, ulong>[] snapshot = UIPlayerID.ToArray();

            foreach (var kvp in snapshot)
            {
                GameObject go = kvp.Key;
                ulong id = kvp.Value;

                if (!go.activeSelf)
                    continue;

                ulong trueID = id;
                if (id == localPLayerID)
                {
                    UIPlayerID[go] = 0;
                    trueID = 0;
                }

                OtherPlayerUIContent otherUI = go.GetComponent<OtherPlayerUIContent>();

                var player = PlayerController.GetPlayer(trueID);

                if (!_subscribedPlayers.Contains(trueID))
                {
                    GameObject slot = go;

                    //player.wood.OnValueChanged += (oldVal, newVal) =>
                    //{
                    //    var txt = otherUI.woodUI;
                    //    txt.text = newVal.ToString();
                    //};
                    //player.food.OnValueChanged += (oldVal, newVal) =>
                    //{
                    //    var txt = otherUI.foodUI;
                    //    txt.text = newVal.ToString();
                    //};
                    //player.health.OnValueChanged += (oldVal, newVal) =>
                    //{
                    //    if (newVal < oldVal)
                    //        for (int i = newVal; i < oldVal; i++)
                    //        {
                    //            otherUI.hearts[i].gameObject.SetActive(false);
                    //        }
                    //    else if (newVal > oldVal)
                    //        for (int i = oldVal; i < newVal; i++)
                    //        {
                    //            otherUI.hearts[i].gameObject.SetActive(true);
                    //        }
                    //};

                    _subscribedPlayers.Add(id);
                }

                var title = go.GetComponentInChildren<TextMeshProUGUI>();
                if (AutoSessionBootstrapper.AutoConnect)
                    title.text = player.name;
                else
                    title.text = ServerManager.Instance.GetPlayerName(player.LocalPlayerId);

            }
        }

        private void SetRitualUI()
        {
            TextMeshProUGUI ritualWood = _ritualObject.transform
                                      .Find("Ritual_Wood_Text")
                                      .GetComponent<TextMeshProUGUI>();
            ritualWood.text = ServerManager.Instance._woodInRitual.Value.ToString() + "/6";
            ServerManager.Instance._woodInRitual.OnValueChanged += (oldVal, newVal) =>
            {
                ritualWood.text = newVal.ToString() + "/6";
            };

            TextMeshProUGUI ritualFood = _ritualObject.transform
                                      .Find("Ritual_Food_Text")
                                      .GetComponent<TextMeshProUGUI>();
            ritualFood.text = ServerManager.Instance._foodInRitual.Value.ToString() + "/6";
            ServerManager.Instance._foodInRitual.OnValueChanged += (oldVal, newVal) =>
            {
                ritualFood.text = newVal.ToString() + "/6";
            };
        }
    }
}