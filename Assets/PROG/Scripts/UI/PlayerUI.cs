using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.Serialization;
using Sirenix.OdinInspector;

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

        [SerializeField] public List<Transform> cardSpaces = new List<Transform>();
        [SerializeField] public List<GameObject> hearts = new List<GameObject>();

        public Dictionary<GameObject, ulong> UIPlayerID = new Dictionary<GameObject, ulong>();

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

        public void SetPlayerInfos()
        {
            foreach (var item in UIPlayerID)
            {
                if (item.Key.activeSelf == false)
                    continue;

                var currentplayer = PlayerController.GetPlayer(item.Value);

                var woodGameObject = item.Key.transform.Find("Ritual_Wood_Text").gameObject;
                var currentWoodText = woodGameObject.GetComponent<TextMeshProUGUI>();
                currentWoodText.text = $"{currentplayer.wood.Value.ToString()}";

                var foodGameObject = item.Key.transform.Find("Ritual_Food_Text").gameObject;
                var currentFoodText = foodGameObject.GetComponent<TextMeshProUGUI>();
                currentFoodText.text = $"{currentplayer.food.Value.ToString()}";

                //playerTitle = item.Key.GetComponentInChildren<TextMeshProUGUI>();
                //ServerManager.Instance.GetPlayerNameRpc(item.Value);
                //playerTitle.text = ServerManager.Instance.playerNameAsked;
            }
        }

        public void DefineFoodText(int foodAmount, ulong target = 0)
        {
            foodCount.text = $"{foodAmount.ToString()}";
        }
        public void DefineWoodText(int woodAmount, ulong target = 0)
        {
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
    }
}