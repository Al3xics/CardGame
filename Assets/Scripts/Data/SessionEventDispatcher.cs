using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wendogo
{
    /// <summary>
    /// A singleton class responsible for dispatching session-related events.
    /// This class allows different parts of the system to register and receive notifications for events
    /// such as joining or leaving a session, and player-related updates within a session.
    /// </summary>
    public class SessionEventDispatcher
    {
        /* ---------- Singleton Creation ---------- */
        /// <summary>
        /// Singleton instance of the <see cref="SessionEventDispatcher"/> class.
        /// Ensures that only one instance of the class exists throughout the application's lifetime.
        /// Provides access to session-related event listeners and their management.
        /// </summary>
        private static SessionEventDispatcher _instance;

        /// <summary>
        /// Gets the singleton instance of the SessionEventDispatcher.
        /// This property provides access to the globally available instance of the SessionEventDispatcher.
        /// If the instance does not already exist, it will be created the first time this property is accessed.
        /// </summary>
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
        /// <summary>
        /// A private field used to store a collection of listeners implementing the
        /// <see cref="ISessionLifecycleEvents"/> interface. These listeners are notified of session
        /// lifecycle events such as when the host joins, leaves, or fails to join a session.
        /// </summary>
        /// <remarks>
        /// Listeners can be added or removed using the `Register` and `Unregister` methods of the
        /// <see cref="SessionEventDispatcher"/> class.
        /// </remarks>
        private readonly List<ISessionLifecycleEvents> _hostListeners = new();

        /// <summary>
        /// A private field used to store a collection of listeners implementing the
        /// <see cref="IPlayerSessionEvents"/> interface. These listeners are notified of session
        /// lifecycle events such as when the host joins, leaves, or fails to join a session.
        /// </summary>
        /// <remarks>
        /// Listeners can be added or removed using the `Register` and `Unregister` methods of the
        /// <see cref="SessionEventDispatcher"/> class.
        /// </remarks>
        private readonly List<IPlayerSessionEvents> _playerListeners = new();

        /// <summary>
        /// Serves as a synchronization object to ensure thread safety
        /// when accessing or modifying shared resources related to session event listeners.
        /// </summary>
        private readonly object _lockObject = new();

        
        /* ---------- Registration API ---------- */
        /// <summary>
        /// Registers a listener object to receive session lifecycle or player session events.
        /// </summary>
        /// <param name="listener">
        /// The listener object to register. It must either implement
        /// <see cref="ISessionLifecycleEvents"/>, <see cref="IPlayerSessionEvents"/>, or both.
        /// </param>
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

        /// <summary>
        /// Unregisters the specified listener from receiving session-related events.
        /// If the listener implements <see cref="ISessionLifecycleEvents"/>, it is removed from
        /// the host listeners collection.
        /// If the listener implements <see cref="IPlayerSessionEvents"/>, it is removed from
        /// the player listeners collection.
        /// </summary>
        /// <param name="listener">
        /// The listener object to unregister. It must either implement
        /// <see cref="ISessionLifecycleEvents"/>, <see cref="IPlayerSessionEvents"/>, or both.
        /// </param>
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
        /// <summary>
        /// Notifies all registered listeners that the host has successfully joined a session.
        /// This method is intended to invoke the <see cref="ISessionLifecycleEvents.OnJoinedSession"/> callback
        /// for each listener currently registered as an <see cref="ISessionLifecycleEvents"/> listener.
        /// </summary>
        /// <remarks>
        /// To register listeners, use the <see cref="SessionEventDispatcher.Register"/> method.
        /// </remarks>
        public void NotifyJoinedSession()
        {
            InvokeForEachListener(_hostListeners, l => l.OnJoinedSession());
        }

        /// <summary>
        /// Notifies all registered listeners that the host has left a session.
        /// This method is intended to invoke the <see cref="ISessionLifecycleEvents.OnLeftSession"/> callback
        /// for each listener currently registered as an <see cref="ISessionLifecycleEvents"/> listener.
        /// </summary>
        /// <remarks>
        /// To register listeners, use the <see cref="SessionEventDispatcher.Register"/> method.
        /// </remarks>
        public void NotifyLeftSession()
        {
            InvokeForEachListener(_hostListeners, l => l.OnLeftSession());
        }

        /// <summary>
        /// Notifies all registered listeners that an attempt to join a session has failed.
        /// This method is intended to invoke the <see cref="ISessionLifecycleEvents.OnFailedToJoinSession"/> callback
        /// for each listener currently registered as an <see cref="ISessionLifecycleEvents"/> listener.
        /// </summary>
        /// <remarks>
        /// To register listeners, use the <see cref="SessionEventDispatcher.Register"/> method.
        /// </remarks>
        public void NotifyFailedToJoinSession()
        {
            InvokeForEachListener(_hostListeners, l => l.OnFailedToJoinSession());
        }

        /// <summary>
        /// Notifies all registered listeners that a player has joined the session.
        /// This method is intended to invoke the <see cref="IPlayerSessionEvents.OnPlayerJoinedSession"/> callback
        /// for each listener currently registered as an <see cref="IPlayerSessionEvents"/> listener.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player who joined the session.</param>
        /// <remarks>
        /// To register listeners, use the <see cref="SessionEventDispatcher.Register"/> method.
        /// </remarks>
        public void NotifyPlayerJoinedSession(string playerId)
        {
            InvokeForEachListener(_playerListeners, l => l.OnPlayerJoinedSession(playerId));
        }

        /// <summary>
        /// Notifies all registered listeners that a specific player has left the session.
        /// This method is intended to invoke the <see cref="IPlayerSessionEvents.OnPlayerLeftSession"/> callback
        /// for each listener currently registered as an <see cref="IPlayerSessionEvents"/> listener.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player who has left the session.</param>
        /// <remarks>
        /// To register listeners, use the <see cref="SessionEventDispatcher.Register"/> method.
        /// </remarks>
        public void NotifyPlayerLeftSession(string playerId)
        {
            InvokeForEachListener(_playerListeners, l => l.OnPlayerLeftSession(playerId));
        }
        
        /// <summary>
        /// Notifies all registered listeners that a specific player has definitively left the session.
        /// This method is intended to invoke the <see cref="IPlayerSessionEvents.OnPlayerLeftSession"/> callback
        /// for each listener currently registered as an <see cref="IPlayerSessionEvents"/> listener.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player who has left the session.</param>
        /// <remarks>
        /// To register listeners, use the <see cref="SessionEventDispatcher.Register"/> method.
        /// </remarks>
        public void NotifyPlayerHasLeftSession(string playerId)
        {
            InvokeForEachListener(_playerListeners, l => l.OnPlayerHasLeftSession(playerId));
        }

        /// <summary>
        /// Notifies all registered listeners that a player's properties have changed.
        /// This method is intended to invoke the <see cref="IPlayerSessionEvents.OnPlayerPropertiesChanged"/> callback
        /// for each listener currently registered as an <see cref="IPlayerSessionEvents"/> listener.
        /// </summary>
        /// <remarks>
        /// To register listeners, use the <see cref="SessionEventDispatcher.Register"/> method.
        /// </remarks>
        public void NotifyPlayerPropertiesChanged()
        {
            InvokeForEachListener(_playerListeners, l => l.OnPlayerPropertiesChanged());
        }

        /// <summary>
        /// Invokes a specified action for each listener in the provided list.
        /// </summary>
        /// <typeparam name="T">The type of the listeners contained in the list.</typeparam>
        /// <param name="listeners">The list of listeners to invoke the action on.</param>
        /// <param name="action">The action to execute for each listener.</param>
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