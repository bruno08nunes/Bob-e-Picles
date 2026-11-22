using Unity.Netcode;
using UnityEngine;

public class MeleeAttack : NetworkBehaviour
{
    [SerializeField] MeleeWeapon weapon;
    [SerializeField] SoundEffectData attackSoundEffect;

    [Rpc(SendTo.Server)]
    public void AttackRpc()
    {
        PlayAttackRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void PlayAttackRpc()
    {
        weapon.Attack();
        Debug.Log(attackSoundEffect);
        SoundManager.Instance.PlaySFX(attackSoundEffect);
        Debug.Log(nameof(SoundManager.Instance.PlaySFX));
        Debug.Log(SoundManager.Instance);
    }
}
