using Unity.Netcode;
using UnityEngine;

public class KeyManager : NetworkBehaviour
{
    [SerializeField] private Color[] colors;

    public static KeyManager Instance { get; private set; }

    private NetworkList<int> keyIds = new NetworkList<int>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public Color GetColor(int id)
    {
        if (id < 0 || id >= colors.Length)
        {
            return Color.white;
        }

        return colors[id];
    }

    public bool HasKey(int id)
    {
        return keyIds.Contains(id);
    }

    public bool GetKey(int id)
    {
        if (!IsServer || id < 0 || id >= colors.Length)
        {
            return false;
        }

        if (keyIds.Contains(id))
        {
            return false;
        }

        keyIds.Add(id);
        return true;
    }
}