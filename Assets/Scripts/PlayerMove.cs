using Unity.Netcode;
using UnityEngine;

public class PlayerMove : NetworkBehaviour
{
    [SerializeField] float speed;

    Vector2 direction;
    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        direction = InputManager.GetMove();
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }

        SendMovementServerRpc(direction);
    }

    [Rpc(SendTo.Server)]
    private void SendMovementServerRpc(Vector2 direction)
    {
        rb.linearVelocity = direction * speed;
    }
}
