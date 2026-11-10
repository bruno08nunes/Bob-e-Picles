using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] float speed = 5f;

    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        rb.linearVelocity = transform.right * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer)
        {
            return;
        }

        if (collision.gameObject.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.TakeDamage(2);
            return;
        }

        if (collision.gameObject.TryGetComponent<Player>(out var player))
        {
            player.TakeDamage(2);
            return;
        }

        GetComponent<NetworkObject>().Despawn();
    }
}