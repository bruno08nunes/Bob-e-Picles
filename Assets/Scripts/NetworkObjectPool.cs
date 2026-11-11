
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

public abstract class NetworkObjectPool<T> : MonoBehaviour, INetworkPrefabInstanceHandler
    where T : NetworkObjectPool<T>
{
    [Header("Pool Settings")]
    [SerializeField] private NetworkObject prefab;
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxSize = 50;

    private static T instance;
    private static ObjectPool<NetworkObject> pool;

    public static NetworkObject Prefab => instance.prefab;

    protected virtual void Awake()
    {
        instance = (T)this;

        pool = new ObjectPool<NetworkObject>(
            CreateObject,
            OnGet,
            OnRelease,
            OnDestroyObject,
            true,
            defaultCapacity,
            maxSize
        );
    }

    private NetworkObject CreateObject()
    {
        return Instantiate(prefab);
    }

    private void OnGet(NetworkObject networkObject)
    {
        networkObject.gameObject.SetActive(true);
    }

    private void OnRelease(NetworkObject networkObject)
    {
        networkObject.gameObject.SetActive(false);
    }

    private void OnDestroyObject(NetworkObject networkObject)
    {
        Destroy(networkObject.gameObject);
    }

    public void Register(NetworkManager networkManager)
    {
        networkManager.PrefabHandler.AddHandler(prefab.gameObject, this);
    }

    public static NetworkObject Get(Vector3 position, Quaternion rotation)
    {
        NetworkObject networkObject = pool.Get();

        networkObject.transform.SetPositionAndRotation(position, rotation);

        return networkObject;
    }

    public static void Return(NetworkObject networkObject)
    {
        if (networkObject.IsSpawned)
            networkObject.Despawn(false);

        pool.Release(networkObject);
    }

    // Called by NGO on clients when the prefab needs to be instantiated.
    public NetworkObject Instantiate(
        ulong ownerClientId,
        Vector3 position,
        Quaternion rotation)
    {
        NetworkObject networkObject = pool.Get();

        networkObject.transform.SetPositionAndRotation(position, rotation);

        return networkObject;
    }

    // Called by NGO on clients when the prefab is despawned.
    public void Destroy(NetworkObject networkObject)
    {
        pool.Release(networkObject);
    }
}
