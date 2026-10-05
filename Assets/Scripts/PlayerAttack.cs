using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class PlayerAttack : NetworkBehaviour
{
    [SerializeField] Transform attackOrigin;

    PlayerMove playerMove;

    SpawnProjectile spawnProjectile;

    void Start()
    {
        playerMove = GetComponent<PlayerMove>();
        spawnProjectile = attackOrigin.GetComponent<SpawnProjectile>();
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (InputManager.WasAttackPressed())
        {
            spawnProjectile.ShootRpc(playerMove.LookDirection);
        }
    }
}
