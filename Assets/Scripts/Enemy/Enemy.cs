using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Enemy : NetworkBehaviour
{
    [SerializeField] private float invulnerabilityDuration = 0.8f;
    [SerializeField] private float flashInterval = 0.1f;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform spriteTransform;

    [SerializeField] private CapsuleCollider2D collider;

    private Color originalColor;
    private Quaternion originalRotation;

    private bool isInvulnerable;
    private bool isDead;

    private float hitTimer;
    private float deathTimer;

    public NetworkVariable<int> Life { get; private set; } = new NetworkVariable<int>(5);

    private void Awake()
    {
        originalColor = spriteRenderer.color;
        originalRotation = spriteTransform.localRotation;
        collider = GetComponent<CapsuleCollider2D>();
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        if (isDead)
        {
            UpdateDeathAnimation();
            return;
        }

        UpdateHitAnimation();
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer || isInvulnerable || isDead || damage <= 0)
        {
            return;
        }

        Life.Value -= damage;

        if (Life.Value <= 0)
        {
            Die();
            return;
        }

        isInvulnerable = true;
        hitTimer = 0.2f;
        StartCoroutine(DamageEffect());
    }

    void Die()
    {
        if (isDead)
        {
            return;
        }

        Destroy(collider);
        isDead = true;
        isInvulnerable = true;
        deathTimer = 0;
        StopAllCoroutines();
        spriteRenderer.color = originalColor;
    }

    private IEnumerator DamageEffect()
    {
        float invulnerabilityTimer = 0f;

        while (invulnerabilityTimer < invulnerabilityDuration)
        {
            spriteRenderer.color = Color.Lerp(originalColor, Color.red, 0.8f);
            yield return new WaitForSeconds(flashInterval);

            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashInterval);

            invulnerabilityTimer += flashInterval * 2f;
        }

        spriteRenderer.color = originalColor;
        isInvulnerable = false;
    }

    private void UpdateHitAnimation()
    {
        if (hitTimer > 0f)
        {
            hitTimer -= Time.deltaTime;

            float shakeSpeed = 35f;
            float shakeAngle = 20f;

            float angle = Mathf.Sin(Time.time * shakeSpeed) * shakeAngle;

            spriteTransform.localRotation =
                originalRotation * Quaternion.Euler(0f, 0f, angle);
        }
        else
        {
            spriteTransform.localRotation = Quaternion.Slerp(
                spriteTransform.localRotation,
                originalRotation,
                10f * Time.deltaTime
            );
        }
    }

    private void UpdateDeathAnimation()
    {
        float dt = Time.deltaTime;
        deathTimer += dt;

        spriteTransform.localPosition +=
            Vector3.down * (0.15f + deathTimer * 0.35f) * dt;

        spriteTransform.localRotation = Quaternion.Slerp(
            spriteTransform.localRotation,
            originalRotation * Quaternion.Euler(0f, 0f, 90f),
            5f * dt
        );

        spriteTransform.localScale = Vector3.Lerp(
            spriteTransform.localScale,
            Vector3.zero,
            3f * dt
        );

        Color color = spriteRenderer.color;
        color.a = Mathf.Lerp(color.a, 0f, 3f * dt);
        spriteRenderer.color = color;

        if (deathTimer >= 1f)
        {
            GetComponent<NetworkObject>().Despawn();
            Destroy(gameObject);
        }
    }
}