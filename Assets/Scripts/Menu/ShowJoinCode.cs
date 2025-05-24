using System;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wendogo.Data;

namespace Wendogo.Menu
{
    public class ShowJoinCode : MonoBehaviour, ISessionLifecycleEvents
    {
        #region Variables

        [Header("References")]
        [Tooltip("The text where you want the code to be shown.")]
        public TMP_Text codeText;
        
        [Tooltip("The button to copy the code.")]
        public Button copyCodeButton;

        #endregion

        private void OnEnable()
        {
            SessionEventDispatcher.Instance.Register(this);
            SetCopyCodeButtonActive();
        }

        private void OnDisable()
        {
            SessionEventDispatcher.Instance.Unregister(this);
        }

        private void Start()
        {
            if(!codeText)
                codeText = GetComponentInChildren<TMP_Text>();
            if(!copyCodeButton)
                copyCodeButton = GetComponentInChildren<Button>();
            
            copyCodeButton.onClick.AddListener(CopySessionCodeToClipboard);
        }

        public void OnJoinedSession()
        {
            codeText.text = SessionManager.Instance.ActiveSession?.Code ?? SessionConstants.NoCode;
            SetCopyCodeButtonActive();
        }

        public void OnLeftSession()
        {
            codeText.text = SessionConstants.NoCode;
            SetCopyCodeButtonActive();
        }

        private void SetCopyCodeButtonActive()
        {
            copyCodeButton.interactable = SessionManager.Instance.ActiveSession?.Code != null;
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