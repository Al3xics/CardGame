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
        public TextMeshProUGUI PlayerTitle;
        public TextMeshProUGUI ReadyText;
        public Button ReadyButton;

        public TextMeshProUGUI EndTest;

        void Start()
        {
            ReadyText.gameObject.SetActive(false);
            ReadyButton.onClick.AddListener(OnReadyButtonClicked);
        }

        public void RenamePlayer(string name)
        {
            PlayerTitle.text = name;
        }

        private void OnReadyButtonClicked()
        {
            ReadyButton.gameObject.SetActive(false);
            ReadyText.gameObject.SetActive(true);

            if (GameNetworkingManager.Instance != null && NetworkManager.Singleton.IsClient)
            {
                GameNetworkingManager.Instance.PlayerReadyServerRpc();
            }
        }

        public void EndValidation()
        {
            EndTest.gameObject.SetActive(true);
        }
    }
}
