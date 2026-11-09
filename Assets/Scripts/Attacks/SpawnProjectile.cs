using Unity.Netcode;
using UnityEngine;

public class SpawnProjectile : NetworkBehaviour
{
    [SerializeField] GameObject projectilePrefab;

    [Rpc(SendTo.Server)]
    public void ShootRpc(Vector2 direction, Vector2 position)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        GameObject projectile = Instantiate(
            projectilePrefab,
            position,
            rotation
        );

        projectile.GetComponent<NetworkObject>().Spawn();
    }
}