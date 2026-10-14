using Unity.Netcode;
using UnityEngine;

public class EnemyChase : NetworkBehaviour
{
    [SerializeField] float speed = 3f;
    [SerializeField] float targedUpdateInterval = 0.25f;
    [SerializeField] Animator animator;

    Rigidbody2D rb;

    Transform target;
    Transform[] players;
    float targetUpdateTimer;

    bool isWalking;

    int hashIsWalking, hashX, hashY;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        hashIsWalking = Animator.StringToHash("IsWalking");
        hashX = Animator.StringToHash("X");
        hashY = Animator.StringToHash("Y");
    }

    void Start()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        players = new Transform[playerObjects.Length];

        for (int i = 0; i < playerObjects.Length; i++)
        {
            players[i] = playerObjects[i].transform;
        }

        FindClosestPlayer();
    }

    void Update()
    {
        targetUpdateTimer += Time.deltaTime;

        if (targetUpdateTimer >= targedUpdateInterval)
        {
            targetUpdateTimer = 0;
            FindClosestPlayer();
        }
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            rb.linearVelocity = Vector2.zero;
            isWalking = false;
            animator.SetBool(hashIsWalking, false);

            return;
        }

        Vector2 direction = ((Vector2)target.position - rb.position).normalized;

        rb.linearVelocity = direction * speed;

        if (!isWalking)
        {
            isWalking = true;
            animator.SetBool(hashIsWalking, true);
        }
    }

    void FindClosestPlayer()
    {
        float closestDistance = Mathf.Infinity;
        Transform closestPlayer = null;

        foreach (Transform player in players)
        {
            if (player == null || !player.gameObject.activeInHierarchy)
            {
                continue;
            }

            float distance = Vector2.Distance(rb.position, player.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        target = closestPlayer;
    }
}
