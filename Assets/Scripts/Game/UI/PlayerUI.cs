using Data;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Wendogo.Menu;

namespace Wendogo
{
    public class PlayerUI : MonoBehaviour
    {
        private TextMeshProUGUI playerTitle;
        private TextMeshProUGUI readyText;
        private Button readyButton;
        private TextMeshProUGUI endText;
        private TextMeshProUGUI roleText;

        void Awake()
        {
            playerTitle = transform.Find("PlayerTitle")?.GetComponent<TextMeshProUGUI>();
            readyText = transform.Find("ReadyText")?.GetComponent<TextMeshProUGUI>();
            readyButton = transform.Find("ReadyButton")?.GetComponent<Button>();
            endText = transform.Find("END")?.GetComponent<TextMeshProUGUI>();
            roleText = transform.Find("PlayerRole")?.GetComponent<TextMeshProUGUI>();

            if (readyButton != null)
            {
                readyButton.onClick.AddListener(OnReadyButtonClicked);
            }

            if (readyText != null) readyText.gameObject.SetActive(false);
            if (endText != null) endText.gameObject.SetActive(false);
        }

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

        private void OnReadyButtonClicked()
        {
            if (readyButton != null) readyButton.gameObject.SetActive(false);
            if (readyText != null) readyText.gameObject.SetActive(true);

            if (GameNetworkingManager.Instance != null && NetworkManager.Singleton.IsClient)
            {
                GameNetworkingManager.Instance.PlayerReadyServerRpc();
            }
        }

        public void EndValidation()
        {
            if (endText != null)
                endText.gameObject.SetActive(true);
        }
    }
}