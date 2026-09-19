using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public NetworkVariable<int> Points { get; private set; } = new NetworkVariable<int>();

    [SerializeField] GameObject collectablePrefab;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            for (float position = -4f; position <= 4f; position += 2)
            {
                var instance = Instantiate(collectablePrefab, new Vector3(position, 0, 0), Quaternion.Euler(new Vector3(0f, 0f, 0f)));
                instance.GetComponent<NetworkObject>().Spawn();
            }
        };
    }

    public void IncrementPoints(int point)
    {
        if (IsServer)
        {
            Points.Value++;
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log(
            $"[{(IsServer ? "SERVER" : "CLIENT")}] " +
            $"GameManager {GetInstanceID()} " +
            $"Points = {Points.Value}"
        );
    }
}
