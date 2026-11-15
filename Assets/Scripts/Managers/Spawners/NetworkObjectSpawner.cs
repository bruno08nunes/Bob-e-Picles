using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkObjectSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform[] positions;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        StartCoroutine(SpawnObjects());
    }

    private IEnumerator SpawnObjects()
    {
        yield return new WaitForSeconds(0.1f);

        foreach (Transform position in positions)
        {
            var instance = Instantiate(
                prefab,
                position.position,
                Quaternion.identity
            );

            instance.GetComponent<NetworkObject>().Spawn(true);
        }
    }
}