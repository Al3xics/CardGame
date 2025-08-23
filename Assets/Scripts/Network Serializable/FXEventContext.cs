using System;
using Unity.Netcode;

namespace Wendogo
{
    [Serializable]
    public class FXEventContext : INetworkSerializable
    {
        #region Variables

        public FXEventType fxType;
        public ulong playerID;
        
        [NonSerialized]
        private PlayerController _cachedPlayer;

        public PlayerController Player
        {
            get
            {
                if (_cachedPlayer == null && NetworkManager.Singleton.IsClient)
                {
                    _cachedPlayer = PlayerController.GetPlayer(playerID);
                    if (_cachedPlayer == null) // It means the player is dead
                        _cachedPlayer = PlayerController.GetDeadPlayer(playerID);
                }
                return _cachedPlayer;
            }
        }

        #endregion

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref fxType);
            serializer.SerializeValue(ref playerID);
        }
    }
}