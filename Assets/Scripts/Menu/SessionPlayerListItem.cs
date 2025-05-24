using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Wendogo.Menu
{
    public class SessionPlayerListItem : MonoBehaviour
    {
        #region Variables

        [Header("References")]
        [Tooltip("Text Mesh Pro where you want to display the player name.")]
        public TMP_Text playerNameText;
        
        [Tooltip("Image used to change color to see which player you are in the session list.")]
        public Image backgroundImage;
        
        [Tooltip("Button used to kick a player out of the session.\nOnly visible for the host.")]
        public Button kickButton;

        private SessionPlayerList _sessionPlayerList;
        private string _playerId;
        private bool _hostCanKickPlayers;
        private bool _isLocalPlayer;
        public bool IsHost { get; private set; }

        #endregion

        public void Initialize(string playerName, string playerId, bool hostCanKickPlayers)
        {
            playerNameText.text = playerName;
            _playerId = playerId;
            _hostCanKickPlayers = hostCanKickPlayers;
            _isLocalPlayer = playerId == SessionManager.Instance.ActiveSession.CurrentPlayer.Id;
            IsHost = SessionManager.Instance.ActiveSession.IsHost;
            
            if (_isLocalPlayer)
                backgroundImage.color = new Color(Random.value, Random.value, Random.value, 150f/255f);
            
            ShowKickButtonIfConditionsAreMet();
            kickButton.onClick.AddListener(OnKickButtonClicked);
        }

        public void Reset()
        {
            kickButton.onClick.RemoveListener(OnKickButtonClicked);
            _playerId = null;
            if (_isLocalPlayer)
                backgroundImage.color = Color.white;
        }

        private void ShowKickButtonIfConditionsAreMet()
        {
            var value = SessionManager.Instance.ActiveSession.IsHost && _hostCanKickPlayers && !_isLocalPlayer;
            kickButton.gameObject.SetActive(value);
        }

        private void OnKickButtonClicked()
        {
            SessionManager.Instance.KickPlayer(_playerId);
        }
    }
}