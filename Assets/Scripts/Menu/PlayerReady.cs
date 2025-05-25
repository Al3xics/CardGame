using System;
using System.Collections.Generic;
using Data;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;
using Wendogo.Data;

namespace Wendogo.Menu
{
    public class PlayerReady : MonoBehaviour, ISessionLifecycleEvents, IPlayerSessionEvents
    {
        #region Variables

        [Header("References")]
        [Tooltip("The play button for the host to start the game.")]
        public Button playButton;
        
        [Tooltip("The toggle button to indicate if the player is ready or not.")]
        public Toggle readyToggle;
        
        [Tooltip("The text that shows the number of players that are ready to start the game.")]
        public TMP_Text counterText;
        
        private const string ReadyText = "Ready";

        #endregion

        private void OnEnable()
        {
            SessionEventDispatcher.Instance.Register(this);
        }

        private void OnDisable()
        {
            SessionEventDispatcher.Instance.Unregister(this);
        }

        private void Start()
        {
            readyToggle.onValueChanged.AddListener(OnReadyToggleChanged);
            ResetValue();
        }

        public void OnJoinedSession()
        {
            OnReadyToggleChanged(readyToggle.isOn);
            SetReadyToggleActive();
            Refresh();
        }

        public void OnLeftSession()
        {
            ResetValue();
        }

        public void OnFailedToJoinSession()
        {
            ResetValue();
        }

        public void OnPlayerJoinedSession(string playerId)
        {
            OnReadyToggleChanged(readyToggle.isOn);
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

        private void SetReadyToggleActive()
        {
            readyToggle.interactable = SessionManager.Instance.ActiveSession != null;
        }

        private void SetCounterText(int current, int max)
        {
            counterText.text = $"{SessionConstants.PlayerReadyPropertyKey} : {current} / {max}";
        }

        private async void OnReadyToggleChanged(bool isOn)
        {
            try
            {
                var playerProperties = new Dictionary<string, PlayerProperty>
                {
                    {SessionConstants.PlayerReadyPropertyKey, new PlayerProperty(isOn.ToString(), VisibilityPropertyOptions.Member)}
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
            
            
            /* -------- Ready counter -------- */
            int readyCount = 0;
            int maxPlayer = session.MaxPlayers;
            
            foreach (var player in session.Players)
            {
                player.Properties.TryGetValue(SessionConstants.PlayerReadyPropertyKey, out var prop);
                if (prop is { Value: "True" }) readyCount++;
            }
            
            SetCounterText(readyCount, maxPlayer);

            
            /* -------- Play-button (host only) -------- */
            if (playButton)
            {
                bool everyoneReady = readyCount == maxPlayer;
                playButton.interactable = session.IsHost && everyoneReady;
            }

            
            /* -------- Toggle state for local player -------- */
            if (readyToggle)
            {
                bool selfReady = session.CurrentPlayer.Properties.TryGetValue(SessionConstants.PlayerReadyPropertyKey, out var selfProp) && selfProp.Value == "True";

                // avoid infinite recursion
                if (readyToggle.isOn != selfReady)
                    readyToggle.SetIsOnWithoutNotify(selfReady);

                readyToggle.interactable = true;
            }
        }

        private void ResetValue()
        {
            SetCounterText(0, 0);
            if (playButton) playButton.interactable = false;
            readyToggle.SetIsOnWithoutNotify(false);
            SetReadyToggleActive();
        }
    }
}