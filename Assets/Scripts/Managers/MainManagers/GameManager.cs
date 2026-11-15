using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public NetworkVariable<int> Points { get; private set; } = new NetworkVariable<int>();

    public bool HasGameStarted { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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
}
