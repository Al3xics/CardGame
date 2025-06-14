namespace Wendogo
{
    /// <summary>
    /// Callbacks that matter only when the host joins/leaves.
    /// </summary>
    public interface ISessionLifecycleEvents
    {
        void OnJoinedSession() { }
        void OnLeftSession() { }
        void OnFailedToJoinSession() { }
    }

    /// <summary>
    /// Callbacks that fire whenever any player joins/leaves.
    /// </summary>
    public interface IPlayerSessionEvents
    {
        void OnPlayerJoinedSession(string playerId) { }
        void OnPlayerLeftSession(string playerId) { }
        void OnPlayerHasLeftSession(string playerId) { }
        void OnPlayerPropertiesChanged() { }
    }
}