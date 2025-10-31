using Photon.Pun;
using UnityEngine;

public class ItemDropManager : MonoBehaviour
{
    public static ItemDropManager _Inst { get; private set; }

    void Awake()
    {
        if (_Inst != null && _Inst != this) { Destroy(this); return; }
        _Inst = this;
    }

    void OnEnable()  => GameEvents.OnRequestItemDrop += HandleItemDrop;
    void OnDisable() => GameEvents.OnRequestItemDrop -= HandleItemDrop;

    // 이벤트 시그니처와 동일하게 bool 반환
    private bool HandleItemDrop(int id, int amount, GameObject user)
    {
        return TryItemDrop(id, amount, user);
    }

    public bool TryItemDrop(int id, int amount, GameObject user)
    {
        if (!PhotonNetwork.InRoom || user == null || amount <= 0) return false;

        if (!ItemManager._Inst || !ItemManager._Inst.ItemDataPoolSO.TryGetItem(id, out var itemData))
            return false;
        if (itemData == null || !itemData.Prefab) return false;

        string prefabPath = $"Items/{itemData.Prefab.name}";
        var res = Resources.Load<GameObject>(prefabPath);
        if (!res)
        {
            Debug.LogError($"[Drop] Resources '{prefabPath}' not found. (Assets/Resources/{prefabPath}.prefab)");
            return false;
        }

        Vector3 dropPos = user.transform.position + user.transform.forward * 1.0f + Vector3.up * 1.0f;
        Quaternion dropRot = itemData.Prefab.transform.rotation;

        object[] instData = new object[] { amount };
        var go = PhotonNetwork.Instantiate(prefabPath, dropPos, dropRot, data: instData);
        return go != null;
    }
}
