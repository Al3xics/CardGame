using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Wendogo
{
    public class CreateSession : MonoBehaviour, ISessionLifecycleEvents
    {
        #region Variables
        
        /* ---------- Multiplayer Configuration --------- */
        [Header("Configuration")]
        [Tooltip("General Multiplayer Configuration.")]
        public MultiplayerConfiguration multiplayerConfiguration;
        
        
        /* ---------- Variables --------- */
        [Header("Current Page")]
        [Tooltip("Button that initiates the session creation process.")]
        public Button createSessionButton;
        
        [Tooltip("Button that go back to the menu.")]
        public Button backToMenuButton;
        
        [Tooltip("Input field for entering the player's name.")]
        public TMP_InputField playerNameInputField;
        
        [Tooltip("Input field for entering the session name.")]
        public TMP_InputField sessionNameInputField;

        
        [Header("Next Page")]
        [Tooltip("GameObject containing the UI elements for the next page.")]
        public GameObject nextUIGameObject;

        private GameObject _currentGameObject;
        
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
            if (nextUIGameObject == null)
                Debug.LogError($"{nameof(nextUIGameObject)} is missing !");
            
            _currentGameObject = gameObject.transform.parent.gameObject;
            
            createSessionButton.onClick.AddListener(EnterSession);
            sessionNameInputField.onEndEdit.AddListener(input =>
            {
                sessionNameInputField.text = SanitizeInput(input);
            });
            playerNameInputField.onEndEdit.AddListener(input =>
            {
                playerNameInputField.text = SanitizeInput(input);
            });
        }

        public void OnFailedToJoinSession()
        {
            sessionNameInputField.text = "";
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
                createSessionButton.interactable = false;
                backToMenuButton.interactable = false;
                await SessionManager.Instance.EnterSession(GetSessionData());

                if (SessionManager.Instance.ActiveSession == null)
                {
                    throw new Exception("Something went wrong. Maybe the session's name you choose is already taken.");
                }

                _currentGameObject.SetActive(false);
                nextUIGameObject.SetActive(true);
                var showSessionInformation = nextUIGameObject.GetComponentInChildren<ShowSessionInformation>();
                if (showSessionInformation) showSessionInformation.Initialize();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create session: {e.Message}");
            }
            finally
            {
                createSessionButton.interactable = true;
                backToMenuButton.interactable = true;
            }
        }
    }
}