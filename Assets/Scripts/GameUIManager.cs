using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameUIManager : NetworkBehaviour
{
    public GameUIManager Instance { get; private set; }

    [SerializeField] TextMeshProUGUI textPoints;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.Points.OnValueChanged += OnPointsChanged;
    }

    override public void OnNetworkSpawn()
    {
        textPoints.text = "Pontos: " + GameManager.Instance.Points.Value.ToString();
    }

    private void OnPointsChanged(int _, int next)
    {
        ChangePointsUIRpc(next);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ChangePointsUIRpc(int points)
    {
        textPoints.text = "Pontos: " + points.ToString();
    }
}
