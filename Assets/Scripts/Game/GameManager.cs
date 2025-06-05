using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wendogo
{
    public class GameManager : NetworkBehaviour
    {
        private void Start()
        {
            if (NetworkManager.Singleton.LocalClientId == OwnerClientId)
            {
                foreach (var player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
                {
                    /*player.AssignRole();*/
                }
            }
        }
    }
}