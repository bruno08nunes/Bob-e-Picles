using Unity.Netcode;
using UnityEngine;

public class PlayerMove : NetworkBehaviour
{
    [SerializeField] float speed;
    [SerializeField] Transform attackOrigin;
    [SerializeField] float attackDistance = 1f;

    Vector2 direction;
    Vector2 lookDirection;

    Rigidbody2D rb;
    SpawnProjectile spawnProjectile;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnProjectile = attackOrigin.GetComponent<SpawnProjectile>();
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        direction = InputManager.GetMove();

        if (direction != Vector2.zero)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                lookDirection = new Vector2(Mathf.Sign(direction.x), 0);
            }
            else
            {
                lookDirection = new Vector2(0, Mathf.Sign(direction.y));
            }
        }

        if (InputManager.WasAttackPressed())
        {
            spawnProjectile.ShootRpc(lookDirection);
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }

        SendMovementServerRpc(direction, lookDirection);
    }

    [Rpc(SendTo.Server)]
    private void SendMovementServerRpc(Vector2 direction, Vector2 lookDirection)
    {
        rb.linearVelocity = direction * speed;

        if (direction == Vector2.zero)
        {
            return;
        }

        attackOrigin.localPosition = lookDirection * attackDistance;

        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        attackOrigin.localRotation = Quaternion.Euler(0, 0, angle);
    }
}