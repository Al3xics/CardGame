using System;
using System.Linq;
using Data;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Wendogo.Data;
using Wendogo.Scriptables;

namespace Wendogo.Menu
{
    public class JoinSessionByCode : MonoBehaviour, ISessionLifecycleEvents, IPlayerSessionEvents
    {
        #region Variables
        
        /* ---------- Multiplayer Configuration --------- */
        [Header("Configuration")]
        [Tooltip("General Multiplayer Configuration.")]
        public MultiplayerConfiguration multiplayerConfiguration;
        
        
        /* ---------- Variables --------- */
        [Header("References")]
        public Button enterSessionButton;
        public Button pasteCodeButton;
        public TMP_InputField playerNameInputField;
        public TMP_InputField enterCodeInputField;

        #endregion
        
        #region Unity Event
        
        [Space(20)]

        [Tooltip("Only called when the player who plays join the session.\nEither the host, or the client.")]
        public UnityEvent onJoinedSession;
        
        [Tooltip("Only called when the player who plays left the session.\nEither the host, or the client.")]
        public UnityEvent onLeftSession;
        
        [Tooltip("Only called when the player who plays left the session.\nEither the host, or the client.")]
        public UnityEvent onFailedToJoinSession;
        
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
            if (enterSessionButton == null)
                Debug.LogError($"{nameof(enterSessionButton)} is missing !");
            if (pasteCodeButton == null)
                Debug.LogError($"{nameof(pasteCodeButton)} is missing !");
            if (playerNameInputField == null)
                Debug.LogError($"{nameof(playerNameInputField)} is missing !");
            if (enterCodeInputField == null)
                Debug.LogError($"{nameof(enterCodeInputField)} is missing !");

            enterSessionButton.onClick.AddListener(EnterSessionByCode);
            enterSessionButton.interactable = false;
            
            enterCodeInputField.onValueChanged.AddListener(value =>
            {
                enterSessionButton.interactable = !string.IsNullOrEmpty(value);
            });
            playerNameInputField.onEndEdit.AddListener(input =>
            {
                playerNameInputField.text = SanitizeInput(input);
            });
            pasteCodeButton.onClick.AddListener(() =>
            {
                enterCodeInputField.text = GUIUtility.systemCopyBuffer;
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
        }

        public void OnFailedToJoinSession()
        {
            SetButtonAndInputField(true);
            onFailedToJoinSession?.Invoke();
        }

        private string SanitizeInput(string input)
        {
            return string.IsNullOrEmpty(input) ? input : string.Concat(input.Where(c => !char.IsWhiteSpace(c)));
        }

        private EnterSessionData GetSessionData()
        {
            var rawPlayerName = playerNameInputField.text;
            
            return new EnterSessionData
            {
                MultiplayerConfiguration = multiplayerConfiguration,
                SessionAction = SessionAction.JoinByCode,
                JoinCode = enterCodeInputField.text,
                PlayerName = string.IsNullOrWhiteSpace(rawPlayerName) ? null : SanitizeInput(rawPlayerName)
            };
        }

        private async void EnterSessionByCode()
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
            enterSessionButton.interactable = value;
            enterCodeInputField.interactable = value;
            pasteCodeButton.interactable = value;
            if (value) enterCodeInputField.text = SessionConstants.NoCode;
        }
    }
}