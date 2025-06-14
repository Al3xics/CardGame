using System.Collections.Generic;
using UnityEngine;

namespace Wendogo
{
    public class SessionPlayerList : MonoBehaviour, ISessionLifecycleEvents, IPlayerSessionEvents
    {
        #region Variables

        [Header("Settings")]
        [Tooltip("If enabled the host will be able to kick players via the UI.")]
        public bool hostCanKickPlayers = true;
        
        [Header("References")]
        [Tooltip("The GameObject that will be instantiated for each player in the session.")]
        public GameObject listItem;
        
        [Tooltip("The parent transform all ListItems will be instantiated under.")]
        public Transform contentRoot;

        private Dictionary<string, SessionPlayerListItem> _playerListItems = new();

        public Dictionary<string, SessionPlayerListItem> PlayerListItems
        {
            get => _playerListItems;
            private set => _playerListItems = value;
        }

        private List<SessionPlayerListItem> _cachedPlayerListItems = new();

        #endregion

        public void OnEnable()
        {
            SessionEventDispatcher.Instance.Register(this);
            UpdatePlayerList();
        }

        public void OnDisable()
        {
            SessionEventDispatcher.Instance.Unregister(this);
            DisableAllPlayerListItems();
        }

        public void OnJoinedSession()
        {
            UpdatePlayerList();
            Debug.Log("[Session Player List] OnJoinedSession called.");
        }

        public void OnLeftSession()
        {
            DisableAllPlayerListItems();
            Debug.Log("[Session Player List] OnLeftSession called.");
        }

        public void OnPlayerJoinedSession(string playerId)
        {
            UpdatePlayerList();
            Debug.Log("[Session Player List] OnPlayerJoinedSession called.");
        }

        public void OnPlayerLeftSession(string playerId)
        {
            if (_playerListItems.TryGetValue(playerId, out var playerListItem))
            {
                playerListItem.Reset();
                playerListItem.gameObject.SetActive(false);
                _cachedPlayerListItems.Add(playerListItem);
                _playerListItems.Remove(playerId);
                Debug.Log("[Session Player List] OnPlayerLeftSession called.");
            }
        }

        private void UpdatePlayerList()
        {
            var session = SessionManager.Instance.ActiveSession;
            if (session == null) return;
            
            // Disable and cache all current items — we'll rebuild the list in the correct order
            DisableAllPlayerListItems();

            foreach (var player in session.Players)
            {
                var playerID = player.Id;
                var playerListItem = GetPlayerListItem(playerID);
                playerListItem.transform.SetSiblingIndex(contentRoot.childCount);
                playerListItem.gameObject.SetActive(true);
                
                var playerName = "Unknown";
                if (player.Properties.TryGetValue(SessionConstants.PlayerNamePropertyKey, out var playerNameProperty))
                {
                    playerName = playerNameProperty.Value;
                }

                playerListItem.Initialize(playerName, playerID, hostCanKickPlayers);
                _playerListItems[playerID] = playerListItem;
                
                bool isReady = player.Properties.TryGetValue(SessionConstants.PlayerReadyPropertyKey, out var prop) && prop.Value == "True";
                playerListItem.UpdateReadyState(isReady);
            }
        }

        private SessionPlayerListItem GetPlayerListItem(string playerId)
        {
            if (_playerListItems.TryGetValue(playerId, out var playerListItem))
                return playerListItem;

            if (_cachedPlayerListItems.Count > 0)
            {
                playerListItem = _cachedPlayerListItems[0];
                _cachedPlayerListItems.RemoveAt(0);
            }
            else
            {
                playerListItem = Instantiate(listItem, contentRoot).GetComponent<SessionPlayerListItem>();
            }
            
            
            _playerListItems.Add(playerId, playerListItem);
            return playerListItem;
        }

        private void DisableAllPlayerListItems()
        {
            foreach (var playerListItem in _playerListItems.Values)
            {
                playerListItem.Reset();
                playerListItem.gameObject.SetActive(false);
                _cachedPlayerListItems.Add(playerListItem);
            }
            
            _playerListItems.Clear();
        }
    }
}