using Unity.Netcode;
using UnityEngine;

public class MeleeAttack : NetworkBehaviour
{
    [SerializeField] MeleeWeapon weapon;

    [Rpc(SendTo.Server)]
    public void AttackRpc()
    {
        PlayAttackRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void PlayAttackRpc()
    {
        weapon.Attack();
    }
}
