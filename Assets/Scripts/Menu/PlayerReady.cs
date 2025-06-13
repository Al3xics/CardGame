using System;
using System.Collections.Generic;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace Wendogo
{
    public class PlayerReady : MonoBehaviour, ISessionLifecycleEvents, IPlayerSessionEvents
    {
        #region Variables

        [Header("References")]
        [Tooltip("The ready button to indicate if the player is ready or not.")]
        public Button playButton;

        private Button _readyButton;
        private SessionPlayerListItem _playerListItem;
        private bool _isReady = false;

        #endregion

        private void OnEnable()
        {
            SessionEventDispatcher.Instance.Register(this);

            _readyButton = GetComponent<Button>();
            if (_readyButton)
                _readyButton.onClick.AddListener(OnButtonClicked);

            if(playButton)
                playButton.onClick.AddListener(OnButtonStartGameClicked);

            // Called manually because the session is already created and this will not be called otherwise.
            OnJoinedSession();
        }

        private void OnDisable()
        {
            SessionEventDispatcher.Instance.Unregister(this);

            ResetValue();
        }

        public void OnJoinedSession()
        {
            UpdatePlayerProperties();
            Refresh();
        }

        public void OnPlayerJoinedSession(string playerId)
        {
            Refresh();
        }

        public void OnPlayerHasLeftSession(string playerId)
        {
            Refresh();
        }

        public void OnPlayerPropertiesChanged()
        {
            Refresh();
        }

        private void OnButtonClicked()
        {
            // On button click, toggle the ready state of the player and update his properties
            _isReady = !_isReady;
            UpdatePlayerProperties();
        }

        private async void UpdatePlayerProperties()
        {
            try
            {
                var playerProperties = new Dictionary<string, PlayerProperty>
                {
                    {SessionConstants.PlayerReadyPropertyKey, new PlayerProperty(_isReady.ToString(), VisibilityPropertyOptions.Member)}
                };

                // This will trigger OnPlayerPropertiesChanged() because it is binding to the same event 'ActiveSession.PlayerPropertiesChanged += OnPlayerPropertiesChanged;'
                SessionManager.Instance.ActiveSession.CurrentPlayer.SetProperties(playerProperties);
                await SessionManager.Instance.ActiveSession.SaveCurrentPlayerDataAsync();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void Refresh()
        {
            var session = SessionManager.Instance.ActiveSession;

            /* -------- No session -------- */
            if (session == null)
            {
                ResetValue();
                return;
            }

            /* -------- Play-button (host only) -------- */
            if (playButton)
            {
                int readyCount = 0;
                foreach (var p in session.Players)
                    if (p.Properties.TryGetValue(SessionConstants.PlayerReadyPropertyKey, out var prop) && prop.Value == "True")
                        readyCount++;

                bool everyoneReady = readyCount == session.MaxPlayers;
                playButton.interactable = session.IsHost && everyoneReady;
            }
        }

        private void OnButtonStartGameClicked()
        {
            if (playButton != null)
                playButton.interactable = false;

            ServerManager.Instance?.LaunchGame();
        }

        private void ResetValue()
        {
            _isReady = false;
            if (playButton != null)
                playButton.interactable = true;

            _readyButton.onClick.RemoveListener(OnButtonClicked);
            playButton?.onClick.RemoveListener(OnButtonStartGameClicked);
        }
    }
}