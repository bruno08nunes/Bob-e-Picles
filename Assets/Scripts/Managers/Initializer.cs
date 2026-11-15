using Unity.Netcode;
using UnityEngine;

public class Initializer : NetworkBehaviour
{
    [SerializeField] NetworkObject gameManager;
    [SerializeField] NetworkObject gameUI;

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
    }
}
