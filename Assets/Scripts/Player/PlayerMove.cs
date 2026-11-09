
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMove : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f;

    [Header("Attack")]
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private float attackDistance = 1f;

    [Header("Visual")]
    [SerializeField] private Animator animator;

    [Header("Network")]
    [SerializeField] private float interpolationBackTime = 0.1f;

    [Header("Prediction Collision")]
    [SerializeField] private LayerMask predictionCollisionLayers = ~0;
    [SerializeField] private float collisionSkin = 0.02f;

    private readonly RaycastHit2D[] predictionHits = new RaycastHit2D[8];
    private ContactFilter2D predictionFilter;

    private Rigidbody2D rb;

    private Vector2 direction;
    private Vector2 lookDirection = Vector2.down;

    public Vector2 LookDirection => lookDirection;
    public Vector2 AttackOriginPosition => attackOrigin.transform.position;

    private Coroutine attackCoroutine;

    public bool IsAttacking { get; private set; }

    public void SetIsAttacking(float duration)
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }

        IsAttacking = true;

        attackCoroutine = StartCoroutine(ResetAttackState(duration));
    }

    private IEnumerator ResetAttackState(float duration)
    {
        yield return new WaitForSeconds(duration);

        IsAttacking = false;
        attackCoroutine = null;
    }

    private int hashIsWalking;
    private int hashX;
    private int hashY;

    // CLIENT
    private uint currentTick;
    private uint lastAcknowledgedTick;

    private readonly InputBuffer inputBuffer = new();

    // Server
    private uint nextServerTick = 1;
    private uint lastProcessedTick;

    private readonly SortedDictionary<uint, PlayerInputData>
        serverInputQueue = new();

    private bool stateDirty;

    private class RemoteSnapshot
    {
        public PlayerState State;
        public float ReceiveTime;
    }

    private readonly List<RemoteSnapshot> snapshots = new();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        predictionFilter = new ContactFilter2D();
        predictionFilter.SetLayerMask(predictionCollisionLayers);
        predictionFilter.useTriggers = false;

        hashIsWalking = Animator.StringToHash("IsWalking");
        hashX = Animator.StringToHash("X");
        hashY = Animator.StringToHash("Y");
    }

    public override void OnNetworkSpawn()
    {
        currentTick = 0;
        lastAcknowledgedTick = 0;

        nextServerTick = 1;
        lastProcessedTick = 0;

        inputBuffer.Clear();
        serverInputQueue.Clear();
        snapshots.Clear();

        rb.simulated = true;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;

        rb.linearVelocity = Vector2.zero;
    }

    public override void OnNetworkDespawn()
    {
        inputBuffer.Clear();
        serverInputQueue.Clear();
        snapshots.Clear();
    }

    private void Update()
    {
        if (!IsSpawned || !IsOwner)
            return;

        CaptureInput();

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (!IsSpawned)
            return;

        if (IsServer)
        {
            if (IsOwner)
            {
                PlayerInputData input = CreateInput();

                SimulateMovement(input);

                lastProcessedTick = input.Tick;
                stateDirty = true;
            }
            else
            {
                ProcessServerInput();
            }

            return;
        }

        if (IsOwner)
        {
            PredictMovement();
        }
    }

    // INPUT

    private void CaptureInput()
    {
        if (IsAttacking)
        {
            direction = Vector2.zero;
            UpdateAnimator(Vector2.zero);
            return;
        }

        direction = Vector2.ClampMagnitude(
            InputManager.GetMove(),
            1f
        );

        if (direction == Vector2.zero)
            return;

        Vector2 newLookDirection;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            newLookDirection = new Vector2(
                Mathf.Sign(direction.x), 0
            );
        }
        else
        {
            newLookDirection = new Vector2(
                0, Mathf.Sign(direction.y)
            );
        }

        if (newLookDirection != lookDirection)
        {
            ApplyLookDirection(newLookDirection);
        }
    }

    private PlayerInputData CreateInput()
    {
        return new PlayerInputData
        {
            Direction = IsAttacking ? Vector2.zero : direction,
            LookDirection = lookDirection,
            Tick = ++currentTick,
            IsAttacking = IsAttacking
        };
    }

    // CLIENT SIDE PREDICTION

    private void PredictMovement()
    {
        PlayerInputData input = CreateInput();

        inputBuffer.Add(input);

        SimulateMovement(input);

        SubmitInputRpc(input);
    }

    private bool HasObstacleAhead(Vector2 moveDirection)
    {
        if (moveDirection.sqrMagnitude < 0.001f)
            return false;

        Vector2 normalizedDirection = moveDirection.normalized;

        float checkDistance =
            speed * Time.fixedDeltaTime + collisionSkin;

        int hitCount = rb.Cast(
            normalizedDirection,
            predictionFilter,
            predictionHits,
            checkDistance
        );

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = predictionHits[i];

            if (hit.collider == null)
                continue;

            // Ignore collisions that are not blocking the movement direction.
            if (Vector2.Dot(hit.normal, normalizedDirection) < -0.01f)
            {
                return true;
            }
        }

        return false;
    }

    // SERVER

    [Rpc(SendTo.Server)]
    private void SubmitInputRpc(PlayerInputData input)
    {
        if (!IsServer)
            return;

        if (input.Tick < nextServerTick || input.Tick > nextServerTick + 120)
        {
            return;
        }

        if (serverInputQueue.ContainsKey(input.Tick))
            return;

        input.Direction = Vector2.ClampMagnitude(input.Direction, 1f);

        serverInputQueue.Add(input.Tick, input);
    }

    private void ProcessServerInput()
    {
        if (!serverInputQueue.TryGetValue(nextServerTick, out PlayerInputData input))
        {
            return;
        }

        serverInputQueue.Remove(nextServerTick);

        SimulateMovement(input);

        lastProcessedTick = input.Tick;

        nextServerTick++;

        stateDirty = true;
    }

    // MOVEMENT SIMULATION

    private void SimulateMovement(PlayerInputData input)
    {
        if (input.IsAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            UpdateAnimator(Vector2.zero);
            return;
        }

        Vector2 movementDirection = input.Direction;

        if (HasObstacleAhead(movementDirection))
        {
            movementDirection = Vector2.zero;
        }

        rb.linearVelocity = movementDirection * speed;

        if (input.Direction != Vector2.zero)
        {
            ApplyLookDirection(input.LookDirection);
        }

        UpdateAnimator(movementDirection);
    }

    // SERVER STATE

    private void LateUpdate()
    {
        if (!IsSpawned)
            return;

        if (IsServer)
        {
            if (stateDirty)
            {
                BroadcastServerState();

                stateDirty = false;
            }

            return;
        }

        if (!IsOwner)
        {
            InterpolateRemotePlayer();
        }
    }

    private void BroadcastServerState()
    {
        PlayerState state = new PlayerState
        {
            Position = rb.position,
            Velocity = rb.linearVelocity,
            LookDirection = lookDirection,
            LastProcessedTick = lastProcessedTick
        };

        SendStateRpc(state);
    }

    [Rpc(SendTo.NotServer, Delivery = RpcDelivery.Unreliable)]
    private void SendStateRpc(PlayerState state)
    {
        if (!IsSpawned)
            return;

        if (IsOwner)
        {
            Reconcile(state);
        }
        else
        {
            AddRemoteSnapshot(state);
        }
    }

    // SERVER RECONCILIATION

    private void Reconcile(PlayerState serverState)
    {
        if (serverState.LastProcessedTick <= lastAcknowledgedTick)
            return;

        lastAcknowledgedTick = serverState.LastProcessedTick;

        inputBuffer.RemoveConfirmed(lastAcknowledgedTick);

        rb.position = serverState.Position;
        rb.linearVelocity = serverState.Velocity;

        Vector2 predictedPosition = serverState.Position;
        Vector2 predictedVelocity = serverState.Velocity;

        foreach (PlayerInputData input in inputBuffer.GetPending())
        {
            rb.position = predictedPosition;

            if (input.IsAttacking || HasObstacleAhead(input.Direction))
            {
                predictedVelocity = Vector2.zero;
            }
            else
            {
                predictedVelocity = input.Direction * speed;
            }

            predictedPosition +=
                predictedVelocity * Time.fixedDeltaTime;
        }

        rb.position = predictedPosition;
        rb.linearVelocity = predictedVelocity;

        rb.position = predictedPosition;
        rb.linearVelocity = predictedVelocity;
    }

    // REMOTE PLAYER INTERPOLATION

    private void AddRemoteSnapshot(PlayerState state)
    {
        snapshots.Add(new RemoteSnapshot
        {
            State = state,
            ReceiveTime = Time.time
        });

        // Limita o tamanho do buffer.
        while (snapshots.Count > 30)
        {
            snapshots.RemoveAt(0);
        }
    }

    private void InterpolateRemotePlayer()
    {
        if (snapshots.Count == 0)
            return;

        float renderTime = Time.time - interpolationBackTime;

        while (snapshots.Count >= 2 && snapshots[1].ReceiveTime <= renderTime)
        {
            snapshots.RemoveAt(0);
        }

        if (snapshots.Count == 1)
        {
            ApplyRemoteState(snapshots[0].State);
            return;
        }

        RemoteSnapshot from = snapshots[0];
        RemoteSnapshot to = snapshots[1];

        float duration = to.ReceiveTime - from.ReceiveTime;

        if (duration <= 0f)
            return;

        float t = Mathf.Clamp01((renderTime - from.ReceiveTime) / duration);

        Vector2 position = Vector2.Lerp(
            from.State.Position,
            to.State.Position,
            t
        );

        transform.position = new Vector3(
            position.x,
            position.y,
            transform.position.z
        );

        Vector2 remoteLookDirection =
            Vector2.Lerp(
                from.State.LookDirection,
                to.State.LookDirection,
                t
            );

        if (remoteLookDirection.sqrMagnitude > 0.001f)
        {
            ApplyLookDirection(remoteLookDirection);
        }

        UpdateAnimator(
            Vector2.Lerp(
                from.State.Velocity,
                to.State.Velocity,
                t
            )
        );
    }

    private void ApplyRemoteState(PlayerState state)
    {
        transform.position = new Vector3(
            state.Position.x,
            state.Position.y,
            transform.position.z
        );

        ApplyLookDirection(state.LookDirection);

        UpdateAnimator(state.Velocity);
    }

    // VISUALS

    private void ApplyLookDirection(Vector2 newLookDirection)
    {
        if (newLookDirection.sqrMagnitude < 0.001f)
            return;

        lookDirection = newLookDirection;

        ChangeAttackOriginPosition(lookDirection);

        ChangePlayerXScale(lookDirection);
    }

    private void UpdateAnimator()
    {
        UpdateAnimator(direction);
    }

    private void UpdateAnimator(Vector2 movement)
    {
        if (animator == null)
            return;

        bool isWalking = movement.sqrMagnitude > 0.001f;

        animator.SetBool(hashIsWalking, isWalking);

        animator.SetFloat(hashX, Mathf.Abs(lookDirection.x));

        animator.SetFloat(hashY, lookDirection.y);
    }

    private void ChangePlayerXScale(Vector2 look)
    {
        Vector3 scale = transform.localScale;

        if (look.x < 0)
        {
            scale.x = -Mathf.Abs(scale.x);
        }
        else if (look.x > 0)
        {
            scale.x = Mathf.Abs(scale.x);
        }

        transform.localScale = scale;
    }

    private void ChangeAttackOriginPosition(Vector2 look)
    {
        if (attackOrigin == null)
            return;

        attackOrigin.localPosition =
            new Vector2(
                Mathf.Abs(look.x),
                look.y
            ) * attackDistance;

        float angle = Mathf.Atan2(
            look.y,
            Mathf.Abs(look.x)
        ) * Mathf.Rad2Deg;

        attackOrigin.localRotation =
            Quaternion.Euler(0, 0, angle);
    }
}
