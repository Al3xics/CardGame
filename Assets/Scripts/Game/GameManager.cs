using Data;
using TMPro;
using UnityEngine;
using Wendogo;
using Wendogo.Menu;

namespace Wendogo
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private PlayerUI playerUIscript;

        void Start()
        {
            SessionManager.Instance.ActiveSession.CurrentPlayer.Properties.TryGetValue(SessionConstants.PlayerNamePropertyKey, out var name);
            playerUIscript?.RenamePlayer(name.Value);
        }

        void Update()
        {
        }

        public void Validation()
        {
            playerUIscript?.EndValidation();
        }
    }
}
