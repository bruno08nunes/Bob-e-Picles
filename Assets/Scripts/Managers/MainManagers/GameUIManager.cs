using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameUIManager : NetworkBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI textPoints;
    [SerializeField] private GameObject[] heartsContainers;
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private GameObject gameOverScreen;

    private List<GameObject>[] playerHearts = new List<GameObject>[2];

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        for (int i = 0; i < playerHearts.Length; i++)
        {
            playerHearts[i] = new List<GameObject>();
        }
    }

    private void Start()
    {
        GameManager.Instance.Points.OnValueChanged += OnPointsChanged;
    }

    public override void OnNetworkSpawn()
    {
        textPoints.text = "Pontos: " + GameManager.Instance.Points.Value;
    }

    private void OnPointsChanged(int _, int next)
    {
        ChangePointsUIRpc(next);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ChangePointsUIRpc(int points)
    {
        textPoints.text = "Pontos: " + points;
    }

    public void InstantiateHearts(int amount, int player)
    {
        if (player < 0 || player >= heartsContainers.Length)
            return;

        GameObject heartContainer = heartsContainers[player];

        if (heartContainer == null)
            return;

        if (playerHearts[player].Count > 0)
        {
            return;
        }

        for (int i = 0; i < amount; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartContainer.transform);
            playerHearts[player].Add(heart);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void RemoveHeartRpc(int amount, int player)
    {
        if (player < 0 || player >= playerHearts.Length)
            return;

        for (int i = 0;  i < amount; i++)
        {
            if (playerHearts[player].Count == 0)
                return;

            int lastIndex = playerHearts[player].Count - 1;

            Destroy(playerHearts[player][lastIndex]);
            playerHearts[player].RemoveAt(lastIndex);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void AddHeartRpc(int player)
    {
        if (player < 0 || player >= heartsContainers.Length)
            return;

        GameObject heartContainer = heartsContainers[player];

        if (heartContainer == null)
            return;

        GameObject heart = Instantiate(heartPrefab, heartContainer.transform);
        playerHearts[player].Add(heart);
    }

    public void EnableGameOverScreen()
    {
        gameOverScreen.SetActive(true);
    }

    public void DisableGameOverScreen()
    {
        gameOverScreen.SetActive(false);
    }
}
