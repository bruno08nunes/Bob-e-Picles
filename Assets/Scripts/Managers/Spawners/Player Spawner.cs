using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject bobPrefab;
    [SerializeField] private NetworkObject piclesPrefab;

    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;

    private void Update()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (!IsServer || GameManager.Instance.HasGameStarted)
        {
            return;
        }

        SpawnPlayers();
    }

    private void SpawnPlayers()
    {
        NetworkManager networkManager = NetworkManager.Singleton;

        var clientIds = networkManager.ConnectedClientsIds;

        foreach (ulong id in clientIds)
        {
            Debug.Log($"[SPAWN] ClientId conectado: {id}");
        }

        if (clientIds.Count != 2)
        {
            Debug.LogError($"Esperados 2 clientes, mas existem {clientIds.Count}.");
        }

        ulong hostId = NetworkManager.ServerClientId;

        ulong secondPlayerId = ulong.MaxValue;

        foreach (ulong id in clientIds)
        {
            if (id != hostId)
            {
                secondPlayerId = id;
                break;
            }
        }

        PlayerCharacterMenuSync characterSync = PlayerCharacterMenuSync.Instance;

        if (characterSync == null || !characterSync.IsSpawned)
        {
            Debug.LogError("PlayerCharacterMenuSync não existe ou não foi spawnado na rede.");

            if (clientIds.Count == 1)
            {
                SpawnPlayer(hostId, 'B', player1SpawnPoint);
            }
            else
            {
                if (secondPlayerId == ulong.MaxValue)
                {
                    Debug.LogError("Não foi possível encontrar o segundo jogador.");
                    return;
                }

                SpawnPlayer(secondPlayerId, 'P', player2SpawnPoint);
            }

            return;
        }

        if (secondPlayerId == ulong.MaxValue)
        {
            Debug.LogError("Não foi possível encontrar o segundo jogador.");
            return;
        }

        SpawnPlayer(hostId, characterSync.Player1Character.Value, player1SpawnPoint);

        SpawnPlayer(secondPlayerId, characterSync.Player2Character.Value, player2SpawnPoint);

        GameManager.Instance.StartGame();
    }

    private void SpawnPlayer(ulong clientId, char character, Transform spawnPoint)
    {
        NetworkObject prefab = character == 'B'
            ? bobPrefab
            : piclesPrefab;

        if (prefab == null || spawnPoint == null)
        {
            Debug.LogError($"Prefab ou ponto de spawn não configurado para o cliente {clientId}.");
            return;
        }

        NetworkObject player = Instantiate(
            prefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        player.SpawnAsPlayerObject(clientId);

        Debug.Log($"Cliente {clientId} recebeu o personagem {character}.");
    }
}
