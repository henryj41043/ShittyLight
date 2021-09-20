using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Connection;
using MLAPI.Messaging;

namespace ShittyLight.Lobby.Networking
{
    public class PlayerSpawnSystem : NetworkBehaviour
    {
        [SerializeField] private NetworkObject playerPrefab = null;
        [SerializeField] private Transform[] spawnPoints;

        private List<Transform> remainingSpawnPoints;
        private List<ulong> loadingClients = new List<ulong>();

        public override void NetworkStart()
        {
            if (IsServer)
            {
                foreach(NetworkClient networkClient in NetworkManager.Singleton.ConnectedClientsList)
                {
                    loadingClients.Add(networkClient.ClientId);
                }
                remainingSpawnPoints = new List<Transform>(spawnPoints);
            }

            if (IsClient)
            {
                ClientIsReadyServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ClientIsReadyServerRpc(ServerRpcParams serverRpcParams = default)
        {
            if (!loadingClients.Contains(serverRpcParams.Receive.SenderClientId)) { return; }
            SpawnPlayer(serverRpcParams.Receive.SenderClientId);
            loadingClients.Remove(serverRpcParams.Receive.SenderClientId);
            if (loadingClients.Count != 0) { return; }
            Debug.Log("Everyone Is Ready");
        }

        private void SpawnPlayer(ulong clientId)
        {
            Transform spawnPoint = remainingSpawnPoints[0];
            remainingSpawnPoints.RemoveAt(0);
            NetworkObject playerInstance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            playerInstance.SpawnAsPlayerObject(clientId, null, true);
        }

        public Transform getRandomSpawnPoint()
        {
            return spawnPoints[(int)Mathf.Round(Random.Range(0f, spawnPoints.Length - 1))];
        }
    }
}
