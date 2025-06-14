using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Wendogo
{
    public class SessionPlayerListItem : MonoBehaviour, ISessionLifecycleEvents, IPlayerSessionEvents
    {
        #region Variables

        [Header("References")]
        [Tooltip("Text Mesh Pro where you want to display the player name.")]
        public TMP_Text playerNameText;
        
        [Tooltip("Image used to change color to see which player you are in the session list.")]
        public Image backgroundImage;
        
        [Tooltip("Button used to kick a player out of the session.\nOnly visible for the host.")]
        public Button kickButton;
        
        [Tooltip("Button used to quit the game.\nOnly visible for the host.")]
        public Button quitButton;
        
        [Header("Colors")]
        [Tooltip("Color to indicate which player you are in the players list.")]
        public Color colorForLocalPlayer = Color.green;
    
        [Tooltip("Color applied to the player's name when they are ready.")]
        public Color readyColor = Color.green;
    
        [Tooltip("Color applied to the player's name when they are NOT ready.")]
        public Color notReadyColor = new(0.8f, 0.3f, 0.3f);


        private string _playerId;
        private bool _hostCanKickPlayers;
        private bool _isLocalPlayer;
        private Color _currentColor;
        private bool IsHost { get; set; }

        #endregion

        #region Unity Event

        [Space(20)]
        
        [Tooltip("Event invoked when the user left the session.")]
        public UnityEvent leftSession = new();

        #endregion

        private void OnEnable()
        {
            SessionEventDispatcher.Instance.Register(this);
        }

        private void OnDisable()
        {
            SessionEventDispatcher.Instance.Unregister(this);
        }

        public void OnLeftSession()
        {
            if (_isLocalPlayer)
                UIManager.Instance.SwitchFromJoinToMainScreen();
            
            leftSession?.Invoke();
            Debug.Log("[SessionPlayerListItem] OnLeftSession called.");
        }
        
        public void OnPlayerPropertiesChanged()
        {
            if (string.IsNullOrEmpty(_playerId)) return;

            // Updates the color if this player's Ready property has changed
            var self = SessionManager.Instance.ActiveSession.Players.FirstOrDefault(p => p.Id == _playerId);

            if (self == null) return;

            bool isReady = self.Properties.TryGetValue(SessionConstants.PlayerReadyPropertyKey, out var prop) && prop.Value == "True";
            UpdateReadyState(isReady);
        }

        public void Initialize(string playerName, string playerId, bool hostCanKickPlayers)
        {
            playerNameText.text = playerName;
            _playerId = playerId;
            _hostCanKickPlayers = hostCanKickPlayers;
            _isLocalPlayer = playerId == SessionManager.Instance.ActiveSession.CurrentPlayer.Id;
            IsHost = SessionManager.Instance.ActiveSession.IsHost;
            
            _currentColor = backgroundImage.color;
            if (_isLocalPlayer)
                backgroundImage.color = colorForLocalPlayer;
            
            ShowKickButtonIfConditionsAreMet();
            kickButton.onClick.AddListener(OnKickButtonClicked);

            ShowQuitButtonIfConditionsAreMet();
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }

        public void Reset()
        {
            kickButton.onClick.RemoveListener(OnKickButtonClicked);
            quitButton.onClick.RemoveListener(OnQuitButtonClicked);
            _playerId = null;
            if (_isLocalPlayer)
                backgroundImage.color = _currentColor;
            UpdateReadyState(false);
        }
        
        public void UpdateReadyState(bool isReady)
        {
            playerNameText.color = isReady ? readyColor : notReadyColor;
        }
        
        private void ShowKickButtonIfConditionsAreMet()
        {
            var value = SessionManager.Instance.ActiveSession.IsHost && _hostCanKickPlayers && !_isLocalPlayer;
            kickButton.gameObject.SetActive(value);
        }

        private void ShowQuitButtonIfConditionsAreMet()
        {
            quitButton.gameObject.SetActive(_isLocalPlayer);
        }
        
        private void OnKickButtonClicked()
        {
            SessionManager.Instance.KickPlayer(_playerId);
        }

        private async void OnQuitButtonClicked()
        {
            await SessionManager.Instance.LeaveSession();
            if (IsHost)
                UIManager.Instance.SwitchFromHostToMainScreen();
            else
                UIManager.Instance.SwitchFromJoinToMainScreen();
        }
    }
}