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

        [SerializeField] private DarkTrickHandler _darkTrickHandler;

        public void TargetSelection()
        {
            OnTargetPicked?.Invoke(_playerID);
        }

        public void Sabotage()
        {
            _darkTrickHandler.PlaydtSabotageCard((ulong)_playerID);
        }

        public void Revalation()
        {
            _darkTrickHandler.PlaydtSeeCard((ulong)_playerID);
        }


    }
}