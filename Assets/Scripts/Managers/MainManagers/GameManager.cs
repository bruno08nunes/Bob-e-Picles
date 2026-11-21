using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public NetworkVariable<int> Points { get; private set; } = new NetworkVariable<int>();

    public bool HasGameStarted { get; private set; } = false;

    private int playersAlive = 2;

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
        GameUIManager.Instance.InstantiateHearts(5, 0);
        GameUIManager.Instance.InstantiateHearts(5, 1);
    }

    public void StartGame()
    {
        HasGameStarted = true;
    }

    public void IncrementPoints(int points)
    {
        if (IsServer)
        {
            Points.Value += points;
        }
    }

    private void ResetValues()
    {
        Points.Value = 0;
        HasGameStarted = false;
        playersAlive = 2;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void InstantiateHeartsRpc()
    {
        GameUIManager.Instance.InstantiateHearts(5, 0);
        GameUIManager.Instance.InstantiateHearts(5, 1);
    }

    public void PlayerDie()
    {
        playersAlive--;
        if (playersAlive == 0)
        {
            SceneController.GoToFirstRoom();
            ResetValues();
            InstantiateHeartsRpc();
        }
    }
}
