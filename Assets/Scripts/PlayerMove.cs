using Unity.Netcode;
using UnityEngine;

public class PlayerMove : NetworkBehaviour
{
    [SerializeField] float speed;
    [SerializeField] Transform attackOrigin;
    [SerializeField] float attackDistance = 1f;
    [SerializeField] Animator animator;

    Vector2 direction;
    public Vector2 LookDirection { get; private set; }

    int hashIsWalking, hashX, hashY;

    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        hashIsWalking = Animator.StringToHash("IsWalking");
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

        bool isWalking = direction != Vector2.zero;
        animator.SetBool(hashIsWalking, isWalking);

        if (direction != Vector2.zero)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                LookDirection = new Vector2(Mathf.Sign(direction.x), 0);
            }
            else
            {
                LookDirection = new Vector2(0, Mathf.Sign(direction.y));
            }
        }

        animator.SetFloat(hashX, Mathf.Abs(LookDirection.x));
        animator.SetFloat(hashY, LookDirection.y);
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }

        SendMovementServerRpc(direction, LookDirection);
    }

    [Rpc(SendTo.Server)]
    private void SendMovementServerRpc(Vector2 direction, Vector2 lookDirection)
    {
        rb.linearVelocity = direction * speed;

        bool isWalking = direction != Vector2.zero;
        if (!isWalking)
        {
            return;
        }

        ChangeAttackOriginPosition(lookDirection);
        ChangePlayerXScale(lookDirection);
    }

    private void ChangePlayerXScale(Vector2 lookDirection)
    {
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

    private void ChangeAttackOriginPosition(Vector2 lookDirection)
    {
        attackOrigin.localPosition = new Vector2(Mathf.Abs(lookDirection.x), lookDirection.y) * attackDistance;
        float angle = Mathf.Atan2(lookDirection.y, Mathf.Abs(lookDirection.x)) * Mathf.Rad2Deg;
        attackOrigin.localRotation = Quaternion.Euler(0, 0, angle);
    }
}