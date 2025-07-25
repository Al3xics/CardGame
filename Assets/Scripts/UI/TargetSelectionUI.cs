using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Wendogo
{
    public class TargetSelectionUI : MonoBehaviour
    {
        [SerializeField] private int _playerID;
        public static event Action<int> OnTargetPicked;

        public void TargetSelection()
        {
            OnTargetPicked?.Invoke(_playerID);
        }

    }
}