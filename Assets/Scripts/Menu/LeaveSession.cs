using Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Wendogo.Data;

namespace Wendogo.Menu
{
    public class LeaveSession : MonoBehaviour, ISessionLifecycleEvents, IPlayerSessionEvents
    {
        #region Variables

        [Tooltip("Event invoked when the user left the session.")]
        public UnityEvent leftSession = new();
        
        private Button _leaveButton;

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
            _leaveButton = GetComponent<Button>();
            _leaveButton.onClick.AddListener(Leave);
            SetLeaveButtonActive();
        }

        public void OnJoinedSession()
        {
            SetLeaveButtonActive();
            Debug.Log("[Leave Session] OnJoinedSession called.");
        }

        public void OnLeftSession()
        {
            leftSession?.Invoke();
            SetLeaveButtonActive();
            Debug.Log("[Leave Session] OnLeftSession called.");
        }

        private void SetLeaveButtonActive()
        {
            _leaveButton.interactable = SessionManager.Instance.ActiveSession != null;
        }

        private async void Leave()
        {
            await SessionManager.Instance.LeaveSession();
        }
    }
}