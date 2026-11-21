using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

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

    [Rpc(SendTo.Server)]
    private void ResetValuesRpc()
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

    [Rpc(SendTo.ClientsAndHost)]
    private void EnableGameOverRpc()
    {
        GameUIManager.Instance.EnableGameOverScreen();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DisableGameOverRpc()
    {
        GameUIManager.Instance.DisableGameOverScreen();
    }

    private void Reset()
    {
        SceneController.GoToFirstRoom();
        ResetValuesRpc();
        InstantiateHeartsRpc();
        DisableGameOverRpc();
    }

    private IEnumerator AnyButtonPressed()
    {
        yield return new WaitForSeconds(2f);
        InputSystem.onAnyButtonPress.CallOnce((InputControl _) =>
        {
            Reset();
        });
    }

    public void PlayerDie()
    {
        playersAlive--;
        if (playersAlive == 0)
        {
            EnableGameOverRpc();
            StartCoroutine(nameof(AnyButtonPressed));
        }
    }
}
