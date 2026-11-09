using Unity.Netcode;
using UnityEngine;

public class PlayerAttack : NetworkBehaviour
{
    [SerializeField] Transform attackOrigin;
    [SerializeField] Animator playerAnimator;
    [SerializeField] float attackDelay = 0.5f;

    int attackHash;

    PlayerMove playerMove;

    SpawnProjectile spawnProjectile;
    MeleeAttack meleeAttack;

    bool isMelee;

    private float nextAttackTime;

    void Start()
    {
        playerMove = GetComponent<PlayerMove>();
        attackHash = Animator.StringToHash("Attack");
        spawnProjectile = attackOrigin.GetComponent<SpawnProjectile>();
        isMelee = spawnProjectile == null;

        if (isMelee)
        {
            meleeAttack = attackOrigin.GetComponent<MeleeAttack>();
        }
    }

    void Update()
    {
        if (!IsOwner)
            return;

        if (Time.time < nextAttackTime)
            return;

        if (!InputManager.WasAttackPressed())
            return;

        nextAttackTime = Time.time + attackDelay;

        AttackRpc(
            playerMove.LookDirection,
            playerMove.AttackOriginPosition
        );
    }

    [Rpc(SendTo.Server)]
    void AttackRpc(Vector2 direction, Vector2 position)
    {
        PlayAttackRpc();

        if (isMelee)
        {
            meleeAttack.AttackRpc();
            return;
        }

        spawnProjectile.ShootRpc(direction, position);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void PlayAttackRpc()
    {
        playerAnimator.SetTrigger(attackHash);
        playerMove.SetIsAttacking(0.5f);
    }
}