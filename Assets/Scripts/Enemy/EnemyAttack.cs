using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] Transform attackOrigin;
    [SerializeField] Animator animator;
    [SerializeField] float attackDelay = 2f;

    int attackHash;
    float attackTime;

    MeleeAttack meleeAttack;

    void Start()
    {
        attackHash = Animator.StringToHash("Attack");
        meleeAttack = attackOrigin.GetComponent<MeleeAttack>();
    }

    private void Update()
    {
        if (attackTime < attackDelay)
        {
            attackTime += Time.deltaTime;
        }
    }

    public void Attack()
    {
        if (attackTime < attackDelay)
        {
            return;
        }
        meleeAttack.AttackRpc();
        animator.SetTrigger(attackHash);
        attackTime = 0;
    }
}
