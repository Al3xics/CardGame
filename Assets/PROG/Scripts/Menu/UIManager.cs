using TMPro;
using UnityEngine;

namespace Wendogo
{
    public class UIManager : MonoBehaviour
    {
        #region Variables

        public static UIManager Instance { get; private set; }
    
        /* ---------- Main Screen Objects --------- */
        [Header("Main Screen Objects")]
        public GameObject mainScreenPage;
        
        [Header("Host Page Objects")]
        [Tooltip("Reference to the main host page container GameObject.")]
        public GameObject hostPage;
        
        [Tooltip("Reference to the create session UI element.")]
        public GameObject createSession;
        
        [Tooltip("Reference to the player list UI element for the host.")]
        public GameObject showSessionPlayerListHost;
        
        [Tooltip("Input field for entering the session name when hosting.")]
        public TMP_InputField sessionNameInputField;

        
        /* ---------- Join Page Objects --------- */
        [Header("Join Page Objects")]
        [Tooltip("Reference to the main join page container GameObject.")]
        public GameObject joinPage;
        
        [Tooltip("Reference to the join session UI element.")]
        public GameObject joinSession;
        
        [Tooltip("Reference to the player list UI element for clients.")]
        public GameObject showSessionPlayerListClient;
        
        [Tooltip("Input field for entering the session code when joining.")]
        public TMP_InputField enterCodeInputField;

        #endregion
    
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }
        
        public void SwitchFromMainScreenToHost()
        {
            if (mainScreenPage != null) mainScreenPage.SetActive(false);
            if (hostPage != null) hostPage.SetActive(true);
        }

        public void SwitchFromMainScreenToJoin()
        {
            if (mainScreenPage != null) mainScreenPage.SetActive(false);
            if (joinPage != null) joinPage.SetActive(true);
        }
    
        public void SwitchFromHostToMainScreen()
        {
            if (hostPage != null) hostPage.SetActive(false);
            if (mainScreenPage != null) mainScreenPage.SetActive(true);
            if (createSession != null) createSession.SetActive(true);
            if (showSessionPlayerListHost != null) showSessionPlayerListHost.SetActive(false);
            if (sessionNameInputField != null) sessionNameInputField.text = "";
        }
    
        public void SwitchFromJoinToMainScreen()
        {
            if (joinPage != null) joinPage.SetActive(false);
            if (mainScreenPage != null) mainScreenPage.SetActive(true);
            if (joinSession != null) joinSession.SetActive(true);
            if (showSessionPlayerListClient != null) showSessionPlayerListClient.SetActive(false);
            if (enterCodeInputField != null) enterCodeInputField.text = "";
        }
        
        
    }
}