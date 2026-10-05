using Unity.Netcode;
using UnityEngine;

public class PlayerMove : NetworkBehaviour
{
    [SerializeField] float speed;
    [SerializeField] Transform attackOrigin;
    [SerializeField] float attackDistance = 1f;

    Vector2 direction;
    Vector2 lookDirection;

    int hashWalking, hashX, hashY;

    Rigidbody2D rb;
    Animator anim;
    SpawnProjectile spawnProjectile;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spawnProjectile = attackOrigin.GetComponent<SpawnProjectile>();

        hashWalking = Animator.StringToHash("IsWalking");
        hashX = Animator.StringToHash("X");
        hashY = Animator.StringToHash("Y");
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        direction = InputManager.GetMove();

        anim.SetBool(hashWalking, direction != Vector2.zero);

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

        anim.SetFloat(hashX, Mathf.Abs(lookDirection.x));
        anim.SetFloat(hashY, lookDirection.y);

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

        //Virar o jogador para o lado certo
        Vector3 scale = transform.localScale;

        if (lookDirection.x < 0)
        {
            scale.x = -Mathf.Abs(scale.x);
        }
        else if (lookDirection.x > 0)
        {
            scale.x = Mathf.Abs(scale.x);
        }

        transform.localScale = scale;
    }
}