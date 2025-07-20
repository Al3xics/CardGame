using UnityEngine;
using Unity.Services.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Analytics;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using Unity.Services.Vivox;

namespace Wendogo
{
    public class SessionManager : MonoBehaviour
    {
        #region Session Manager Instance

        private static SessionManager _instance;

        public static SessionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    CreateInstance();
                }
                return _instance;
            }
        }

        private static void CreateInstance()
        {
            var gameObject = new GameObject($"{nameof(SessionManager)}");
            _instance = gameObject.AddComponent<SessionManager>();
            DontDestroyOnLoad(gameObject);
        }

        #endregion
        
        #region Variables
        
        private static bool _isInitialized = false;

        private ISession _activeSession;
        public ISession ActiveSession
        {
            get => _activeSession;
            private set
            {
                if (value != null)
                {
                    _activeSession = value;
                    RegisterSessionEvents();
                    SessionEventDispatcher.Instance.NotifyJoinedSession();
                }
                else if (_activeSession != null)
                {
                    _activeSession = null;
                    SessionEventDispatcher.Instance.NotifyLeftSession();
                }
            }
        }
        public EnterSessionData EnterSessionData { get; set; }
        
        private IAuthenticationService _authenticationService;
        public IAuthenticationService AuthenticationService
        {
            get { return _authenticationService ??= Unity.Services.Authentication.AuthenticationService.Instance; }
            private set => _authenticationService = value;
        }
        
        private IMultiplayerService _multiplayerService;
        public IMultiplayerService MultiplayerService
        {
            get { return _multiplayerService ??= Unity.Services.Multiplayer.MultiplayerService.Instance; }
            private set => _multiplayerService = value;
        }
        
        private IVivoxService _vivoxService;
        public IVivoxService VivoxService
        {
            get { return _vivoxService ??= Unity.Services.Vivox.VivoxService.Instance; }
            private set => _vivoxService = value;
        }

        #endregion

        private void Awake()
        {
            if (_instance != null) return;
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _ = SessionEventDispatcher.Instance;
        }
        
        private async void Start()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            
            try
            {
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    await UnityServices.InitializeAsync();
                    Debug.Log("Initialized Unity Services");
                }

                if (!AuthenticationService.IsSignedIn)
                {
                    await AuthenticationService.SignInAnonymouslyAsync();
                    Debug.Log($"Signed in anonymously. Name: {AuthenticationService.PlayerName}. ID: {AuthenticationService.PlayerId}");
                }
                
                if (VivoxService is { IsLoggedIn: false })
                {
                    await VivoxService.InitializeAsync();
                    Debug.Log("Initialized Voice Chat");
                }
                
                AnalyticsManager.Instance.StartDataCollection();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public async Task EnterSession(EnterSessionData sessionData)
        {
            try
            {
                EnterSessionData = sessionData;
                var playerProperties = await GetPlayerProperties(sessionData.PlayerName);

                // Create Session Options
                var options = new SessionOptions
                {
                    Name = sessionData.SessionName,
                    MaxPlayers = sessionData.MultiplayerConfiguration.maxPlayers,
                    IsLocked = sessionData.MultiplayerConfiguration.isLocked,
                    IsPrivate = sessionData.MultiplayerConfiguration.isPrivate,
                    PlayerProperties = playerProperties
                };

                // Join Session Options
                var joinSessionOptions = new JoinSessionOptions()
                {
                    PlayerProperties = playerProperties
                };
                
                // Voice Chat Options
                if (sessionData.MultiplayerConfiguration.enableVoiceChat && VivoxService is { IsLoggedIn: false })
                {
                    var voiceLoginOptions = new LoginOptions()
                    {
                        DisplayName = AuthenticationService.PlayerName,
                        EnableTTS = true
                    };
                    await VivoxService.LoginAsync(voiceLoginOptions);
                }

                SetConnection(ref options, sessionData.MultiplayerConfiguration);

                switch (sessionData.SessionAction)
                {
                    case SessionAction.Create:
                        ActiveSession = await MultiplayerService.CreateSessionAsync(options);
                        AnalyticsManager.Instance.RecordEvent(new CustomEvent("lobbyCreated"));
                        break;
                    case SessionAction.JoinByCode:
                        ActiveSession = await MultiplayerService.JoinSessionByCodeAsync(sessionData.JoinCode, joinSessionOptions);
                        AnalyticsManager.Instance.RecordEvent(new CustomEvent("lobbyJoined"));
                        break;
                    case SessionAction.Invalid:
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (sessionData.MultiplayerConfiguration.enableVoiceChat && VivoxService != null && !VivoxService.ActiveChannels.ContainsKey(ActiveSession.Name))
                {
                    await VivoxService.JoinGroupChannelAsync(ActiveSession.Name, ChatCapability.AudioOnly);
                    AnalyticsManager.Instance.RecordEvent(new CustomEvent("vivoxGroupChannelJoined"));
                }
            }
            catch (SessionException e)
            {
                HandleSessionException();
                Debug.LogError(e);
            }
            catch (AggregateException ae)
            {
                ae.Handle(ex =>
                {
                    if (ex is SessionException sessionException)
                    {
                        HandleSessionException();
                        return true;
                    }

                    return false;
                });
            }
        }

        private void HandleSessionException()
        {
            SessionEventDispatcher.Instance.NotifyFailedToJoinSession();
            ActiveSession = null;
        }

        private async Task<Dictionary<string, PlayerProperty>> GetPlayerProperties(string playerName = null)
        {
            var pName = string.IsNullOrEmpty(playerName) ? await AuthenticationService.GetPlayerNameAsync() : playerName;
            var playerNameProperties = new PlayerProperty(pName, VisibilityPropertyOptions.Member);
            var playerProperties = new Dictionary<string, PlayerProperty>{ {SessionConstants.PlayerNamePropertyKey, playerNameProperties} };
            return playerProperties;
        }

        private void SetConnection(ref SessionOptions sessionOptions, MultiplayerConfiguration multiConfig)
        {
            switch (multiConfig.connectionType)
            {
                case ConnectionType.None:
                    break;
                case ConnectionType.Direct:
                    sessionOptions.WithDirectNetwork(multiConfig.listenIpAddress, 
                        multiConfig.connectionMode == ConnectionMode.Listen ? multiConfig.listenIpAddress : multiConfig.publishIpAddress, multiConfig.port);
                    break;
#if NGO_2_AVAILABLE                  
                case ConnectionType.DistributedAuthority:
                    options.WithDistributedAuthorityNetwork();
                    break;
#endif
                case ConnectionType.Relay:
                default:
                    sessionOptions.WithRelayNetwork();
                    break;
            }
        }

        private void RegisterSessionEvents()
        {
            ActiveSession.PlayerJoined += OnPlayerJoined;
            ActiveSession.PlayerLeaving += OnPlayerLeaving;
            ActiveSession.PlayerHasLeft += OnPlayerHasLeft;
            ActiveSession.PlayerPropertiesChanged += OnPlayerPropertiesChanged;
            ActiveSession.RemovedFromSession += OnRemovedFromSession;
        }
        
        private void UnregisterSessionEvents()
        {
            ActiveSession.PlayerJoined -= OnPlayerJoined;
            ActiveSession.PlayerLeaving -= OnPlayerLeaving;
            ActiveSession.PlayerPropertiesChanged -= OnPlayerPropertiesChanged;
            ActiveSession.RemovedFromSession -= OnRemovedFromSession;
        }
        
        private void OnPlayerJoined(string playerId)
        {
            Debug.Log($"[SessionManager] Player joined: {playerId}");
            SessionEventDispatcher.Instance.NotifyPlayerJoinedSession(playerId);
        }
        
        private void OnPlayerLeaving(string playerId)
        {
            Debug.Log($"[SessionManager] Player leaving: {playerId}");
            SessionEventDispatcher.Instance.NotifyPlayerLeftSession(playerId);
        }
        
        private void OnPlayerHasLeft(string playerId)
        {
            Debug.Log($"[SessionManager] Player has left: {playerId}");
            SessionEventDispatcher.Instance.NotifyPlayerHasLeftSession(playerId);
        }

        private void OnRemovedFromSession()
        {
            Debug.Log("[SessionManager] Removed from session");
            _ = LeaveVoiceChannel();
            UnregisterSessionEvents();
            ActiveSession = null;
        }

        private void OnPlayerPropertiesChanged()
        {
            Debug.Log("[SessionManager] Player Properties Changed");
            SessionEventDispatcher.Instance.NotifyPlayerPropertiesChanged();
        }

        public async void KickPlayer(string playerId)
        {
            try
            {
                if (!ActiveSession.IsHost) return;
            
                await ActiveSession.AsHost().RemovePlayerAsync(playerId);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        public async Task LeaveSession()
        {
            if (ActiveSession == null) return;
            bool isHost = ActiveSession.IsHost;
            
            try
            {
                // As host, notify all players before leaving
                // This will trigger the RemovedFromSession event for all clients
                if (isHost)
                {
                    // 1. Take a snapshot WITHOUT the host
                    var others = new List<IReadOnlyPlayer>(ActiveSession.Players);
                    others.RemoveAll(p => p.Id == ActiveSession.CurrentPlayer.Id);

                    // 2. Exclude them one by one
                    foreach (var p in others)
                    {
                        try
                        {
                            await ActiveSession.AsHost().RemovePlayerAsync(p.Id);
                        }
                        catch (Exception kickEx)
                        {
                            Debug.LogWarning($"Kick failed for {p.Id} : {kickEx.Message}");
                        }
                    }
                    
                    // Small delay to allow players to leave the voice channel (thanks to `OnRemovedFromSession`)
                    await Task.Delay(1500);
                }
                
                await LeaveVoiceChannel();
                UnregisterSessionEvents();
                await ActiveSession.LeaveAsync();
                
                AnalyticsManager.Instance.RecordEvent(new CustomEvent("lobbyLeft"));
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
            finally
            {
                ActiveSession = null;
            }
        }
        
        public void MutePlayer(bool mute)
        {
            if (mute)
            {
                if (!VivoxService.IsInputDeviceMuted)
                {
                    VivoxService.MuteInputDevice();
                    Debug.Log("[Vivox] Input device muted.");
                }
            }
            else
            {
                if (VivoxService.IsInputDeviceMuted)
                {
                    VivoxService.UnmuteInputDevice();
                    Debug.Log("[Vivox] Input device unmuted.");
                }
            }
        }

        private async Task LeaveVoiceChannel()
        {
            try
            {
                await VivoxService.LeaveChannelAsync(ActiveSession.Name);
                Debug.Log("[Vivox] Left voice channel.");
                
                AnalyticsManager.Instance.RecordEvent(new CustomEvent("vivoxGroupChannelLeft"));
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Vivox] LeaveVoiceChannel failed: {ex.Message}");
            }
        }

        private async void OnApplicationQuit()
        {
            try
            {
                if (VivoxService is { IsLoggedIn: true })
                {
                    foreach (var kvp in VivoxService.ActiveChannels)
                    {
                        await VivoxService.LeaveChannelAsync(kvp.Key);
                        Debug.Log($"[Vivox] Left voice channel '{kvp.Key}' on application quit.");
                    }
                    
                    await VivoxService.LogoutAsync();
                    Debug.Log("[Vivox] Disconnected at application quit.");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Vivox] Logout failed on quit: {e.Message}");
            }
        }
    }
}