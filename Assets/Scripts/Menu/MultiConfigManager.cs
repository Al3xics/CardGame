using UnityEngine;

namespace Wendogo
{
    public class MultiConfigManager : MonoBehaviour
    {
        public static MultiConfigManager Instance { get; private set; }
        
        [Header("Configuration")]
        [Tooltip("General Multiplayer Configuration.")]
        public MultiplayerConfiguration multiplayerConfiguration;
        
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }
    }
}