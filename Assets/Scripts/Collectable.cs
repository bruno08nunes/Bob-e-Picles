using Unity.Netcode;
using UnityEngine;

public class Collectable : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        CollectRpc();
    }

    [Rpc(SendTo.Server)]
    void CollectRpc()
    {
        GameManager.Instance.IncrementPoints(10);

        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
