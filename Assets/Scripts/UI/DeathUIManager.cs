using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Wendogo
{
    public class DeathUIManager : MonoBehaviour
    {
        public static DeathUIManager Instance { get; private set; }

        [Header("Canvas to wait before cleanup")]
        public List<GameObject> canvasesToWatch;

        private void Awake()
        {
            Instance = this;
        }

        public async UniTask WaitUntilAllDisabled()
        {
            // Wait until all canvases are disabled
            await UniTask.WaitUntil(() => canvasesToWatch.TrueForAll(c => !c.activeSelf));
        }
    }
}