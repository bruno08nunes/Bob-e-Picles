using Unity.Netcode;
using UnityEngine;

public class Collectable : NetworkBehaviour
{
    [SerializeField] Sprite[] sprites;
    [SerializeField] SoundEffectData collectSoundEffect;

    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (sprites.Length == 0) return;

        int index = Random.Range(0, sprites.Length);

        spriteRenderer.sprite = sprites[index];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CollectRpc();
    }

    [Rpc(SendTo.Server)]
    void CollectRpc()
    {
        GameManager.Instance.IncrementPoints(10);

        PlaySoundRpc();

        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlaySoundRpc()
    {
        SoundManager.Instance.PlaySFX(collectSoundEffect);
    }
}
