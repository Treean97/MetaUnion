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

    public bool TryItemDrop(int id, int amount, GameObject user)
    {
        ItemManager._Inst.ItemDataPoolSO.TryGetItem(id, out var itemData);

        if (itemData == null)
        {
            return false;
        }

        Vector3 dropPos = user.transform.position
        + user.transform.forward * 1.0f
        + Vector3.up * 1.0f;
        
        string prefabPath = $"Items/{itemData.Prefab.name}";      
        var prefabRot = itemData.Prefab.transform.rotation;

        // 네트워크 동기화된 인스턴스 생성
        object[] instData = new object[] { amount };
        PhotonNetwork.Instantiate(
            prefabPath,
            dropPos,
            prefabRot,
            data: instData
        );  


        return true;
    }
}
