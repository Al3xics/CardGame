using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Wendogo
{
    public class TargetSelectionUI : MonoBehaviour
    {
        [SerializeField] private ulong _playerID;
        public static event Action<ulong> OnTargetPicked;

        public void TargetSelection()
        {
            OnTargetPicked?.Invoke(_playerID);
        }

    }
}