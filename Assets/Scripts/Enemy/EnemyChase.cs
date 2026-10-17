using UnityEngine;
using UnityEngine.AI;

public class EnemyChase : MonoBehaviour
{
    [SerializeField] private float targetUpdateInterval = 0.25f;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;


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
            lastDirection = velocity.normalized;

            if (spriteRenderer != null && Mathf.Abs(velocity.x) > 0.01f)
                spriteRenderer.flipX = velocity.x < 0f;
        }

        if (walking != isWalking)
        {
            isWalking = walking;
            animator.SetBool(hashIsWalking, isWalking);
        }

        animator.SetFloat(hashX, Mathf.Abs(lastDirection.x));
        animator.SetFloat(hashY, lastDirection.y);
    }
}