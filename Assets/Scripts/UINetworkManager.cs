using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UINetworkManager : MonoBehaviour
{
    [SerializeField] private Button joinButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button serverButton;

    [SerializeField] private GameObject NetworkCanvas;

    private void Awake()
    {
        joinButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            NetworkCanvas.SetActive(false);
        });

        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            NetworkCanvas.SetActive(false);
        });

        serverButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
            NetworkCanvas.SetActive(false);
        });
    }
}
