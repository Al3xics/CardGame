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

        [SerializeField] private TextMeshProUGUI foodCountP2;

        [SerializeField] public List<GameObject> hearts = new List<GameObject>();

        public Dictionary<GameObject, ulong> UIPlayerID = new Dictionary<GameObject, ulong>();
        public Dictionary<Transform, GameObject> CardSpaces = new Dictionary<Transform, GameObject>();

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
        public void SetPlayerInfos(ulong localPLayerID,RpcParams rpcParams)
        {
            KeyValuePair<GameObject, ulong>[] snapshot = UIPlayerID.ToArray();

            foreach (var kvp in snapshot)
            {
                GameObject go = kvp.Key;
                ulong value = kvp.Value;

                if (!go.activeSelf)
                    continue;

                if (value == localPLayerID)
                {
                    UIPlayerID[go] = 0;
                }

                var player = PlayerController.GetPlayer(value);
                var woodText = go.transform
                                 .Find("Ritual_Wood_Text")
                                 .GetComponent<TextMeshProUGUI>();
                woodText.text = player.wood.Value.ToString();

                var foodText = go.transform
                                 .Find("Ritual_Food_Text")
                                 .GetComponent<TextMeshProUGUI>();
                foodText.text = player.food.Value.ToString();

                var title = go.GetComponentInChildren<TextMeshProUGUI>();
                title.text = player.name;
            }
        }

    }
}