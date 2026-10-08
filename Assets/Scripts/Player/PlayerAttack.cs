using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class PlayerAttack : NetworkBehaviour
{
    [SerializeField] Transform attackOrigin;
    [SerializeField] Animator playerAnimator;

    int attackHash;

    PlayerMove playerMove;

    SpawnProjectile spawnProjectile;
    MeleeAttack meleeAttack;

    bool isMelee;

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
        {
            return;
        }

        if (!InputManager.WasAttackPressed())
        {
            return;
        }

        if (isMelee)
        {
            meleeAttack.AttackRpc();
            playerAnimator.SetTrigger(attackHash);
            return;
        }

        spawnProjectile.ShootRpc(playerMove.LookDirection);
    }
}
