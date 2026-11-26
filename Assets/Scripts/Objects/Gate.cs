using UnityEngine;

public class Gate : MonoBehaviour
{
    [SerializeField] int id;
    [SerializeField] SpriteRenderer lockSprite;

    void Start()
    {
        if (KeyManager.Instance.HasKey(id))
        {
            Destroy(gameObject);
        }

        lockSprite.color = KeyManager.Instance.GetColor(id);
    }
}
