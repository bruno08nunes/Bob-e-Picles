
using UnityEngine;
using UnityEngine.AI;

public class EnemyChase : MonoBehaviour
{
    [SerializeField] private float targetUpdateInterval = 0.25f;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private float attackDistance = 1f;

    private EnemyAttack enemyAttack;
    private NavMeshAgent agent;
    private Transform[] players;
    private Transform target;
    private float timer;

    private bool isWalking;

    private int hashIsWalking;
    private int hashX;
    private int hashY;

    private Vector2 lastDirection = Vector2.down;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyAttack = GetComponent<EnemyAttack>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        hashIsWalking = Animator.StringToHash("IsWalking");
        hashX = Animator.StringToHash("X");
        hashY = Animator.StringToHash("Y");
    }

    private void Start()
    {
        GameObject[] objects =
            GameObject.FindGameObjectsWithTag("Player");

        players = new Transform[objects.Length];

        for (int i = 0; i < objects.Length; i++)
            players[i] = objects[i].transform;

        FindClosestPlayer();
        UpdateDestination();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= targetUpdateInterval)
        {
            timer = 0f;
            FindClosestPlayer();
            UpdateDestination();
        }

        if (target != null)
        {
            float distance =
                ((Vector2)target.position -
                 (Vector2)transform.position).sqrMagnitude;

            if (distance < 5f)
                enemyAttack.Attack();
        }

        UpdateAnimation();
    }

    private void FindClosestPlayer()
    {
        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (Transform player in players)
        {
            if (player == null ||
                !player.gameObject.activeInHierarchy)
                continue;

            float distance =
                ((Vector2)player.position -
                 (Vector2)transform.position).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = player;
            }
        }

        target = closest;
    }

    private void UpdateDestination()
    {
        if (target != null && agent.isOnNavMesh)
            agent.SetDestination(target.position);
        else if (agent.isOnNavMesh)
            agent.ResetPath();
    }

    private void UpdateAnimation()
    {
        if (animator == null)
            return;

        Vector2 velocity = agent.velocity;
        bool walking = velocity.sqrMagnitude > 0.01f;

        if (walking)
        {
            if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
                lastDirection = new Vector2(Mathf.Sign(velocity.x), 0f);
            else
                lastDirection = new Vector2(0f, Mathf.Sign(velocity.y));

            if (spriteRenderer != null && Mathf.Abs(lastDirection.x) > 0f)
                spriteRenderer.flipX = lastDirection.x < 0f;
        }

        if (walking != isWalking)
        {
            isWalking = walking;
            animator.SetBool(hashIsWalking, isWalking);
        }

        animator.SetFloat(hashX, Mathf.Abs(lastDirection.x));
        animator.SetFloat(hashY, lastDirection.y);

        ChangeAttackOriginPosition(lastDirection);
    }

    private void ChangeAttackOriginPosition(Vector2 lookDirection)
    {
        if (attackOrigin == null)
            return;

        attackOrigin.localPosition = lookDirection * attackDistance;

        float angle = Mathf.Atan2(
            lookDirection.y,
            lookDirection.x
        ) * Mathf.Rad2Deg;

        attackOrigin.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}
