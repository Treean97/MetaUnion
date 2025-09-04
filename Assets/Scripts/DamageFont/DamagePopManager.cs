using UnityEngine;

public class DamagePopManager : MonoBehaviour
{
    public static DamagePopManager _Inst { get; private set; }

    [SerializeField] private DamagePop _Prefab;

    void Awake()
    {
        if (_Inst != null) { Destroy(this); return; }
        _Inst = this;
    }

    public void Show(Vector3 worldPos, int damage)
    {
        var popup = ObjectPoolManager._Inst.Rent(_Prefab); // 풀에서 꺼냄(없으면 생성)
        popup.transform.position = worldPos;
        popup.Play(worldPos, damage, p => ObjectPoolManager._Inst.Return(p.gameObject)); // 사용 후 반납
    }
}
