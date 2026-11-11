
using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifetime = 3f;

    private Rigidbody2D rb;
    private Coroutine lifetimeCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        rb.linearVelocity = transform.right * speed;

        lifetimeCoroutine = StartCoroutine(ReturnToPool());
    }

    public override void OnNetworkDespawn()
    {
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer || !IsSpawned)
            return;

        if (collision.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.TakeDamage(2);
            ReturnProjectile();
            return;
        }

        if (collision.TryGetComponent<Player>(out var player))
        {
            player.TakeDamage(2);
            ReturnProjectile();
            return;
        }

        ReturnProjectile();
    }

    private IEnumerator ReturnToPool()
    {
        yield return new WaitForSeconds(lifetime);

        ReturnProjectile();
    }

    private void ReturnProjectile()
    {
        if (!IsServer || !IsSpawned)
            return;

        PlayerProjectileObjectPool.Return(NetworkObject);
    }
}
