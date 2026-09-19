using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UINetworkManager : MonoBehaviour
{
    [SerializeField] private Button joinButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button serverButton;

    [SerializeField] private GameObject NetworkCanvas;
    [SerializeField] private GameObject GameUICanvas;

    private void Awake()
    {
        joinButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            NetworkCanvas.SetActive(false);
            GameUICanvas.SetActive(true);
        });

        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            NetworkCanvas.SetActive(false);
            GameUICanvas.SetActive(true);
        });

        serverButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
            NetworkCanvas.SetActive(false);
            GameUICanvas.SetActive(true);
        });
    }
}
