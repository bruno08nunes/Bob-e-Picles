using Unity.Netcode;
using UnityEngine;

public class Collectable : NetworkBehaviour
{
    [SerializeField] Sprite[] sprites;

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

        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
