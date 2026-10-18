using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private float invulnerabilityDuration = 0.8f;
    [SerializeField] private float flashInterval = 0.1f;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform spriteTransform;
    private new CapsuleCollider2D collider;

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

        StartCoroutine(InvulnerabilityCooldown());
        DamageClientRpc();
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        isInvulnerable = true;
        deathTimer = 0f;

        StopAllCoroutines();

        spriteRenderer.color = originalColor;

        collider.enabled = false;
        DeathClientRpc();
    }

    [ClientRpc]
    private void DamageClientRpc()
    {
        hitTimer = 0.2f;

        StartCoroutine(DamageFlashEffect());
    }

    private IEnumerator InvulnerabilityCooldown()
    {
        yield return new WaitForSeconds(invulnerabilityDuration);

        isInvulnerable = false;
    }

    private IEnumerator DamageFlashEffect()
    {
        float timer = 0f;

        while (timer < invulnerabilityDuration && !isDead)
        {
            spriteRenderer.color = Color.Lerp(originalColor, Color.red, 0.8f);

            yield return new WaitForSeconds(flashInterval);

            if (isDead)
            {
                break;
            }

            spriteRenderer.color = originalColor;

            yield return new WaitForSeconds(flashInterval);

            timer += flashInterval * 2f;
        }

        if (!isDead)
        {
            spriteRenderer.color = originalColor;
        }
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

    [ClientRpc]
    private void DeathClientRpc()
    {
        isDead = true;
        isInvulnerable = true;
        deathTimer = 0f;

        StopAllCoroutines();

        spriteRenderer.color = originalColor;
        collider.enabled = false;
    }

    private void UpdateDeathAnimation()
    {
        float dt = Time.deltaTime;

        deathTimer += dt;

        spriteTransform.localPosition = (0.15f + deathTimer * 0.35f) * dt * Vector3.down;

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

        if (deathTimer >= 1f && IsServer)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}