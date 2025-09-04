using UnityEngine;

public class PooledObject : MonoBehaviour
{
    [HideInInspector] public ObjectPoolManager _Owner;
    [HideInInspector] public GameObject _Prefab;

    public void Despawn()
    {
        if (_Owner) _Owner.Return(gameObject);
        else Destroy(gameObject); // fallback
    }
}
