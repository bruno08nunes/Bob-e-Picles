using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameUIManager : NetworkBehaviour
{
    [SerializeField] TextMeshProUGUI textPoints;

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
        textPoints.text = "Pontos: " + next.ToString();
    }
}
