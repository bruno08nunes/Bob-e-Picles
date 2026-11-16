
using UnityEngine;

public class PlayerProjectileObjectPool :
    NetworkObjectPool<PlayerProjectileObjectPool>
{
    public PlayerProjectileObjectPool Instance { get; private set; }

    override protected void Awake()
    {
        //if (Instance != null)
        //{
        //    Destroy(gameObject);
        //    return;
        //}

        //Instance = this;
        //DontDestroyOnLoad(gameObject);

        //base.Awake();
    }
}
