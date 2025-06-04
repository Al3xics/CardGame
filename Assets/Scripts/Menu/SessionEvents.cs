using System;
using UnityEngine;
using UnityEngine.Events;
using Wendogo.Data;

namespace Wendogo.Menu
{
    public class SessionEvents : MonoBehaviour, ISessionLifecycleEvents, IPlayerSessionEvents
    {
        #region Unity Event
        
        [Space(20)]

        [Tooltip("Only called when the player who plays join the session.\nEither the host, or the client.")]
        public UnityEvent onJoinedSession;
        
        [Tooltip("Only called when the player who plays left the session.\nEither the host, or the client.")]
        public UnityEvent onLeftSession;
        
        [Tooltip("Only called when the player who plays failed to join the session.\nEither the host, or the client.")]
        public UnityEvent onFailedToJoinSession;

        [Tooltip("Called when any player (except host) join the session created by the host.")]
        public UnityEvent onPlayerJoinedSession;
        
        [Tooltip("Called when any player (except host) left the session he was in.")]
        public UnityEvent onPlayerLeftSession;
        
        [Tooltip("Called when any player (except host) has totally left the session, and the host has to be notified.")]
        public UnityEvent onPlayerHasLeftSession;
        
        [Tooltip("Called when any player properties has changed, including the host.")]
        public UnityEvent onPlayerPropertiesChanged;
        
        #endregion

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public void OnJoinedSession()
        {
            onJoinedSession?.Invoke();
        }

        public void OnLeftSession()
        {
            onLeftSession?.Invoke();
        }

        public void OnFailedToJoinSession()
        {
            onFailedToJoinSession?.Invoke();
        }

        public void OnPlayerJoinedSession(string playerId)
        {
            onPlayerJoinedSession?.Invoke();
        }

        public void OnPlayerLeftSession(string playerId)
        {
            onPlayerLeftSession?.Invoke();
        }

        public void OnPlayerPropertiesChanged()
        {
            onPlayerPropertiesChanged?.Invoke();
        }
    }
}