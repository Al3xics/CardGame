using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Wendogo
{
    public class JoinSessionByCode : MonoBehaviour, ISessionLifecycleEvents
    {
        #region Variables
        
        /* ---------- Multiplayer Configuration --------- */
        [Header("Configuration")]
        [Tooltip("General Multiplayer Configuration.")]
        public MultiplayerConfiguration multiplayerConfiguration;
        
        
        /* ---------- Variables --------- */
        [Header("Current Page")]
        [Tooltip("Button that joins the session.")]
        public Button joinButton;
        
        [Tooltip("Button that go back to the menu.")]
        public Button backToMenuButton;
        
        [Tooltip("Button that paste the session code from your clipboard.")]
        public Button pasteCodeButton;
        
        [Tooltip("Input field for entering the player's name.")]
        public TMP_InputField playerNameInputField;
        
        [Tooltip("Input field for entering the session code.")]
        public TMP_InputField enterCodeInputField;
        
        
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
            if (joinButton == null)
                Debug.LogError($"{nameof(joinButton)} is missing !");
            if (pasteCodeButton == null)
                Debug.LogError($"{nameof(pasteCodeButton)} is missing !");
            if (playerNameInputField == null)
                Debug.LogError($"{nameof(playerNameInputField)} is missing !");
            if (enterCodeInputField == null)
                Debug.LogError($"{nameof(enterCodeInputField)} is missing !");
            if (nextUIGameObject == null)
                Debug.LogError($"{nameof(nextUIGameObject)} is missing !");
            
            _currentGameObject = gameObject.transform.parent.gameObject;

            joinButton.onClick.AddListener(EnterSessionByCode);
            joinButton.interactable = false;
            
            enterCodeInputField.onValueChanged.AddListener(value =>
            {
                joinButton.interactable = !string.IsNullOrEmpty(value);
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

        public void OnFailedToJoinSession()
        {
            enterCodeInputField.text = "";
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
                joinButton.interactable = false;
                backToMenuButton.interactable = false;
                await SessionManager.Instance.EnterSession(GetSessionData());

                if (SessionManager.Instance.ActiveSession == null)
                {
                    throw new Exception("The code is invalid.");
                }
                
                _currentGameObject.SetActive(false);
                nextUIGameObject.SetActive(true);
                var showSessionInformation = nextUIGameObject.GetComponentInChildren<ShowSessionInformation>();
                if (showSessionInformation) showSessionInformation.Initialize();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to join session: {e.Message}");
            }
            finally
            {
                joinButton.interactable = true;
                backToMenuButton.interactable = true;
            }
        }
    }
}