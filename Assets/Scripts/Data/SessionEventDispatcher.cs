using System.Collections.Generic;
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

        
        /* ---------- Registration API ---------- */
        public void Register(object listener)
        {
            if (listener is ISessionLifecycleEvents host && !_hostListeners.Contains(host))
                _hostListeners.Add(host);

            if (listener is IPlayerSessionEvents player && !_playerListeners.Contains(player))
                _playerListeners.Add(player);
        }

        public void Unregister(object listener)
        {
            if (listener is ISessionLifecycleEvents host)
                _hostListeners.Remove(host);
            
            if (listener is IPlayerSessionEvents player)
                _playerListeners.Remove(player);
        }

        
        /* ---------- Dispatch helpers used by SessionManager ---------- */
        public void NotifyJoinedSession() => _hostListeners.ForEach(l => l.OnJoinedSession());
        public void NotifyLeftSession() => _hostListeners.ForEach(l => l.OnLeftSession());
        public void NotifyFailedToJoinSession() => _hostListeners.ForEach(l => l.OnFailedToJoinSession());
        public void NotifyPlayerJoinedSession(string playerId) => _playerListeners.ForEach(l => l.OnPlayerJoinedSession(playerId));
        public void NotifyPlayerLeftSession(string playerId) => _playerListeners.ForEach(l => l.OnPlayerLeftSession(playerId));
        public void NotifyPlayerHasLeftSession(string playerId) => _playerListeners.ForEach(l => l.OnPlayerHasLeftSession(playerId));
        public void NotifyPlayerPropertiesChanged() => _playerListeners.ForEach(l => l.OnPlayerPropertiesChanged());
    }
}