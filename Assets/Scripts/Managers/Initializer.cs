using Unity.Netcode;
using UnityEngine;

public class Initializer : NetworkBehaviour
{
    [SerializeField] NetworkObject gameManager;
    [SerializeField] NetworkObject gameUI;
    [SerializeField] NetworkObject keyManager;

    private void Start()
    {
        if (!IsServer)
        {
            return;
        }

        NetworkObject gameManagerObject = Instantiate(gameManager);
        gameManagerObject.Spawn();

        NetworkObject gameUIObject = Instantiate(gameUI);
        gameUIObject.Spawn();

        NetworkObject keyManagerObject = Instantiate(keyManager);
        keyManagerObject.Spawn();
    }
}
