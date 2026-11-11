using Unity.Netcode;
using UnityEngine;

public class SpawnProjectile : NetworkBehaviour
{
    [Rpc(SendTo.Server)]
    public void ShootRpc(Vector2 direction, Vector2 position)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        NetworkObject projectile = PlayerProjectileObjectPool.Get(transform.position, rotation);

        projectile.Spawn();
    }
}