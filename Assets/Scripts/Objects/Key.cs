using Unity.Netcode;
using UnityEngine;

public class Key : NetworkBehaviour
{
    [SerializeField] int id;
    [SerializeField] SoundEffectData collectSoundEffect;
    [SerializeField] SpriteRenderer sprite;

    void Start()
    {
        if (KeyManager.Instance.HasKey(id))
        {
            Destroy(gameObject);
        }

        sprite.color = KeyManager.Instance.GetColor(id);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CollectRpc();
            return;
        }
    }

    [Rpc(SendTo.Server)]
    void CollectRpc()
    {
        KeyManager.Instance.GetKey(id);

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
