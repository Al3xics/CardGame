using System;
using Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Wendogo.Menu
{
    public class ShowSessionInformation : MonoBehaviour
    {
        #region Variables

        [Header("References")]
        [Tooltip("Text component displaying the player's name.")]
        public TMP_Text playerNameText;
        
        [Tooltip("Text component displaying the session name.")]
        public TMP_Text sessionNameText;
        
        [Tooltip("Text component displaying the session code.")]
        public TMP_Text codeText;
        
        [Tooltip("Button that copies the session code to clipboard.")]
        public Button copyCodeButton;

        #endregion

        private void Awake()
        {
            if (playerNameText == null)
                Debug.LogError($"{nameof(playerNameText)} is missing !");
            if (sessionNameText == null)
                Debug.LogError($"{nameof(sessionNameText)} is missing !");
            if (codeText == null)
                Debug.LogError($"{nameof(codeText)} is missing !");
        }

        public void Initialize()
        {
            playerNameText.text = SessionManager.Instance.ActiveSession.CurrentPlayer.Properties[SessionConstants.PlayerNamePropertyKey].Value;
            sessionNameText.text = SessionManager.Instance.ActiveSession.Name;
            codeText.text = SessionManager.Instance.ActiveSession.Code;
            if (copyCodeButton)
                copyCodeButton.onClick.AddListener(CopySessionCodeToClipboard);
        }

        private void CopySessionCodeToClipboard()
        {
            // Deselect the button when clicked.
            EventSystem.current.SetSelectedGameObject(null);
            
            var code = codeText.text;

            if (SessionManager.Instance.ActiveSession?.Code == null || string.IsNullOrEmpty(code))
            {
                return;
            }

            // Copy the text to the clipboard.
            GUIUtility.systemCopyBuffer = code;
        }
    }
}