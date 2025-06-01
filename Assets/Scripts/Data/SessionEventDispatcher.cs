using System;
using System.Collections.Generic;
using UnityEngine;
using Wendogo.Data;

namespace Data
{
    public class SessionEventDispatcher
    {
        /* ---------- Singleton Creation ---------- */
        private static SessionEventDispatcher _instance;

        public static SessionEventDispatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SessionEventDispatcher();
                }
                return _instance;
            }
        }

        
        /* ---------- Listener storage ---------- */
        private readonly List<ISessionLifecycleEvents> _hostListeners = new();
        private readonly List<IPlayerSessionEvents> _playerListeners = new();
        private readonly object _lockObject = new();

        
        /* ---------- Registration API ---------- */
        public void Register(object listener)
        {
            lock (_lockObject)
            {
                if (listener is ISessionLifecycleEvents host && !_hostListeners.Contains(host))
                    _hostListeners.Add(host);

                if (listener is IPlayerSessionEvents player && !_playerListeners.Contains(player))
                    _playerListeners.Add(player);
            }
        }

        public void Unregister(object listener)
        {
            lock (_lockObject)
            {
                if (listener is ISessionLifecycleEvents host)
                    _hostListeners.Remove(host);
            
                if (listener is IPlayerSessionEvents player)
                    _playerListeners.Remove(player);
            }
        }

        
        /* ---------- Dispatch helpers used by SessionManager ---------- */
        public void NotifyJoinedSession()
        {
            InvokeForEachListener(_hostListeners, l => l.OnJoinedSession());
        }

        public void NotifyLeftSession()
        {
            InvokeForEachListener(_hostListeners, l => l.OnLeftSession());
        }

        public void NotifyFailedToJoinSession()
        {
            InvokeForEachListener(_hostListeners, l => l.OnFailedToJoinSession());
        }

        public void NotifyPlayerJoinedSession(string playerId)
        {
            InvokeForEachListener(_playerListeners, l => l.OnPlayerJoinedSession(playerId));
        }

        public void NotifyPlayerLeftSession(string playerId)
        {
            InvokeForEachListener(_playerListeners, l => l.OnPlayerLeftSession(playerId));
        }
        
        public void NotifyPlayerHasLeftSession(string playerId)
        {
            InvokeForEachListener(_playerListeners, l => l.OnPlayerHasLeftSession(playerId));
        }

        public void NotifyPlayerPropertiesChanged()
        {
            InvokeForEachListener(_playerListeners, l => l.OnPlayerPropertiesChanged());
        }

        private void InvokeForEachListener<T>(List<T> listeners, System.Action<T> action)
        {
            T[] listenersCopy;
            lock (_lockObject)
            {
                listenersCopy = listeners.ToArray();
            }

            foreach (var listener in listenersCopy)
            {
                try
                {
                    action(listener);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error invoking listener: {e}");
                }
            }
        }

    }
}