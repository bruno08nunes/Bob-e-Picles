using Unity.Netcode;
using UnityEngine;

public class MeleeWeapon : NetworkBehaviour
{
    Animator anim;

    int hashAttack;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        hashAttack = Animator.StringToHash("Attack");
    }

    private void Update()
    {
        AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);

        if (state.normalizedTime >= 1f)
        {
            gameObject.SetActive(false);
        }
    }

    public void Attack()
    {
        gameObject.SetActive(true);

        anim.SetTrigger(hashAttack);
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
    }
}
