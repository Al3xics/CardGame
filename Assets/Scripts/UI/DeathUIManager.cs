using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Wendogo
{
    public class DeathUIManager : MonoBehaviour
    {
        public static DeathUIManager Instance { get; private set; }

        private readonly List<GameObject> _uiObjectsToWatch = new();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        /// <summary>
        /// Add a new UI to the list of objects to watch.
        /// </summary>
        public void RegisterUI(GameObject uiInstance)
        {
            if (!_uiObjectsToWatch.Contains(uiInstance))
                _uiObjectsToWatch.Add(uiInstance);
        }

        /// <summary>
        /// Remove a UI in the list of objects to watch.
        /// </summary>
        public void UnregisterUI(GameObject uiInstance)
        {
            if (_uiObjectsToWatch.Contains(uiInstance))
                _uiObjectsToWatch.Remove(uiInstance);
        }

        /// <summary>
        /// Wait until all UI objects are destroyed.
        /// </summary>
        public async UniTask WaitUntilAllDestroyed()
        {
            await UniTask.WaitUntil(() =>
            {
                _uiObjectsToWatch.RemoveAll(obj => obj == null || !obj.activeSelf);
                return _uiObjectsToWatch.Count == 0;
            });
        }
    }
}