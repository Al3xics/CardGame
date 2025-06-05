using NUnit.Framework;
using System;
using System.Data;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Wendogo
{
    public enum RoleType { Survivor, Wendogo };

    public class PlayerController : NetworkBehaviour
    {
        [Header("UI Prefab")]
        public GameObject playerPanelPrefab;

        private PlayerUI playerUIInstance;
        private bool uiInitialized = false;

        public NetworkVariable<RoleType> Role = new (
            value: RoleType.Survivor,
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server
            );

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private new void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!IsOwner || uiInitialized) return;

            if (scene.name == "Night_Day_Mech")
            {
                GameObject mainCanvas = GameObject.Find("MainCanvas");
                if (mainCanvas == null)
                {
                    Debug.LogError("MainCanvas non trouvé !");
                    return;
                }

                GameObject uiObject = Instantiate(playerPanelPrefab, mainCanvas.transform);
                playerUIInstance = uiObject.GetComponent<PlayerUI>();

                if (playerUIInstance != null)
                {
                    playerUIInstance.RenamePlayer(GameNetworkingManager.Instance.GetPlayerName());
                    uiInitialized = true;
                }
            }
        }

        /*public void AssignRole()
        {
            GameNetworkingManager.Instance.AskRoleServerRpc();
        }*/

        [ClientRpc]
        public void NotifyGameReadyClientRpc()
        {
            if (IsOwner && playerUIInstance != null)
            {
                playerUIInstance.EndValidation();
            }
        }

        [ClientRpc]
        public void SendRoleClientRpc(RoleType role, ClientRpcParams clientRpcParams = default)
        {
            playerUIInstance.GetRole(role.ToString());
        }

        [ClientRpc]
        public void SendCardsToClientRpc(List<int> role, ClientRpcParams clientRpcParams = default)
        {
            playerUIInstance.GetRole(role.ToString());
        }
    }
}