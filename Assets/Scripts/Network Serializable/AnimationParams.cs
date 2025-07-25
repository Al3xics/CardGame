using System;
using Unity.Netcode;
using UnityEngine.Serialization;

namespace Wendogo
{
    [Serializable]
    public class AnimationParams : INetworkSerializable
    {
        public AnimatorName animatorName = AnimatorName.None;
        public bool waitForAnimation = false;
        public string trigger = string.Empty;

        public ulong playerId = 0;              // Used in PlayerTurn
        public bool isSurvivorWin = false;      // Used in EndGame

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref animatorName);
            serializer.SerializeValue(ref waitForAnimation);
            serializer.SerializeValue(ref trigger);
            serializer.SerializeValue(ref playerId);
            serializer.SerializeValue(ref isSurvivorWin);
        }
    }
}