using System;
using System.Linq;
using Data;
using JetBrains.Annotations;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Wendogo.Scriptables;
using Wendogo.Data;

namespace Wendogo.Menu
{
    public class CreateSession : MonoBehaviour, ISessionLifecycleEvents, IPlayerSessionEvents
    {
        #region Variables
        
        /* ---------- Multiplayer Configuration --------- */
        [Header("Configuration")]
        [Tooltip("General Multiplayer Configuration.")]
        public MultiplayerConfiguration multiplayerConfiguration;
        
        
        /* ---------- Variables --------- */
        [Header("References")]
        public Button createSessionButton;
        public TMP_InputField playerNameInputField;
        public TMP_InputField sessionNameInputField;

        #endregion
        
        #region Unity Event
        
        [Space(20)]

        [Tooltip("Only called when the player who plays join the session.\nEither the host, or the client.")]
        public UnityEvent onJoinedSession;
        
        [Tooltip("Only called when the player who plays left the session.\nEither the host, or the client.")]
        public UnityEvent onLeftSession;

        [Tooltip("Called when any player (except host) join the session created by the host.")]
        public UnityEvent onPlayerJoinedSession;
        
        [Tooltip("Called when any player (except host) left the session he was in.")]
        public UnityEvent onPlayerLeftSession;
        
        #endregion
        
        private void OnEnable()
        {
            SessionEventDispatcher.Instance.Register(this);
        }

        private void OnDisable()
        {
            SessionEventDispatcher.Instance.Unregister(this);
        }

        private void Awake()
        {
            // Check variables are set in inspector
            if (createSessionButton == null)
                Debug.LogError($"{nameof(createSessionButton)} is missing !");
            if (sessionNameInputField == null)
                Debug.LogError($"{nameof(sessionNameInputField)} is missing !");
            if (playerNameInputField == null)
                Debug.LogError($"{nameof(playerNameInputField)} is missing !");
            
            createSessionButton.onClick.AddListener(EnterSession);
            createSessionButton.interactable = false;
            
            sessionNameInputField.onValueChanged.AddListener(value =>
            {
                createSessionButton.interactable = !string.IsNullOrEmpty(value);
            });
            sessionNameInputField.onEndEdit.AddListener(input =>
            {
                sessionNameInputField.text = SanitizeInput(input);
            });
            playerNameInputField.onEndEdit.AddListener(input =>
            {
                playerNameInputField.text = SanitizeInput(input);
            });
        }

        public void OnJoinedSession()
        {
            onJoinedSession?.Invoke();
        }

        public void OnLeftSession()
        {
            SetButtonAndInputField(true);
            onLeftSession?.Invoke();
            Debug.Log("[Create Session] OnLeftSession called.");
        }

        public void OnPlayerJoinedSession(string playerId)
        {
            onPlayerJoinedSession?.Invoke();
            Debug.Log("[Create Session] OnPlayerJoinedSession called.");
        }

        public void OnPlayerLeftSession(string playerId)
        {
            onPlayerLeftSession?.Invoke();
            Debug.Log("[Create Session] OnPlayerLeftSession called.");
        }

        private string SanitizeInput(string input)
        {
            return string.IsNullOrEmpty(input) ? input : string.Concat(input.Where(c => !char.IsWhiteSpace(c)));
        }

        private EnterSessionData GetSessionData()
        {
            var rawSessionName = sessionNameInputField.text;
            var rawPlayerName = playerNameInputField.text;
            
            return new EnterSessionData()
            {
                MultiplayerConfiguration = multiplayerConfiguration,
                SessionAction = SessionAction.Create,
                SessionName = string.IsNullOrWhiteSpace(rawSessionName) ? multiplayerConfiguration.sessionName : SanitizeInput(rawSessionName),
                PlayerName = string.IsNullOrWhiteSpace(rawPlayerName) ? SessionConstants.HostName : SanitizeInput(rawPlayerName)
            };
        }

        private async void EnterSession()
        {
            try
            {
                SetButtonAndInputField(false);
                await SessionManager.Instance.EnterSession(GetSessionData());
            }
            catch (SessionException e)
            {
                SetButtonAndInputField(true);
                Debug.LogError(e);
            }
        }
        
        private void SetButtonAndInputField(bool value)
        {
            createSessionButton.interactable = value;
            sessionNameInputField.interactable = value;
        }
    }
}