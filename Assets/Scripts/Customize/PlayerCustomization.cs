using System.Linq;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerCustomization : MonoBehaviourPun
{
    [SerializeField] private List<SlotBinding> _SlotBindings;
    private Dictionary<ItemType, SkinnedMeshRenderer> _RendererSlots;

    [System.Serializable]
    public class SlotBinding
    {
        public ItemType Type;
        public SkinnedMeshRenderer Renderer;
    }

    void Awake()
    {
        _RendererSlots = new();
        foreach (var binding in _SlotBindings)
        {
            if (!_RendererSlots.ContainsKey(binding.Type))
                _RendererSlots.Add(binding.Type, binding.Renderer);
        }
    }

    public void EquipItem(CustomizeItemSO itemSO)
    {
        if (!photonView.IsMine) return;

        // 🔁 모든 클라이언트에 동기화 요청
        photonView.RPC(nameof(RPC_EquipMesh), RpcTarget.AllBuffered, (int)itemSO.Type, itemSO.ID);
    }

    [PunRPC]
    private void RPC_EquipMesh(int typeInt, string itemId)
    {
        var type = (ItemType)typeInt;

        var itemPool = CustomizeItemPoolLocator._Inst;
        if (itemPool == null)
        {
            Debug.LogError("CustomizeItemPoolSO가 등록되지 않았습니다.");
            return;
        }

        var item = itemPool.GetItems(type).FirstOrDefault(i => i.ID == itemId);
        if (item == null)
        {
            Debug.LogWarning($"ID '{itemId}'에 해당하는 아이템을 찾을 수 없습니다.");
            return;
        }

        if (!_RendererSlots.TryGetValue(type, out var renderer))
        {
            Debug.LogError($"Renderer가 존재하지 않습니다: {type}");
            return;
        }

        // ✅ 메쉬 교체
        renderer.sharedMesh = item.ItemMesh;
    }
}
