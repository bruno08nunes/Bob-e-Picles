using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public NetworkVariable<int> Points { get; private set; } = new NetworkVariable<int>();

    [SerializeField] GameObject collectablePrefab;
    [SerializeField] GameObject enemyPrefab;

    [SerializeField] private NetworkObject bobPrefab;
    [SerializeField] private NetworkObject piclesPrefab;

    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        SpawnPlayers();

        return;

        for (float position = -4f; position <= 4f; position += 2)
        {
            var instance = Instantiate(collectablePrefab, new Vector3(position, 4, 0), Quaternion.Euler(new Vector3(0f, 0f, 0f)));
            instance.GetComponent<NetworkObject>().Spawn();
        }

        for (float position = -4f; position <= 4f; position += 8)
        {
            var instance = Instantiate(enemyPrefab, new Vector3(position, 1, 0), Quaternion.Euler(new Vector3(0f, 0f, 0f)));
            instance.GetComponent<NetworkObject>().Spawn();
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void IncrementPoints(int point)
    {
        if (IsServer)
        {
            Points.Value++;
        }
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
            return;
        }

        PlayerCharacterMenuSync characterSync = PlayerCharacterMenuSync.Instance;

        if (characterSync == null || !characterSync.IsSpawned)
        {
            Debug.LogError("PlayerCharacterMenuSync não existe ou não foi spawnado na rede.");
            return;
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

        if (secondPlayerId == ulong.MaxValue)
        {
            Debug.LogError("Não foi possível encontrar o segundo jogador.");
            return;
        }

        SpawnPlayer(hostId, characterSync.Player1Character.Value, player1SpawnPoint);

        SpawnPlayer(secondPlayerId, characterSync.Player2Character.Value, player2SpawnPoint);
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
