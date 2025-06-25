using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerCustomization : MonoBehaviourPunCallbacks
{
    [Serializable]
    public class SlotBinding
    {
        public ItemType Type;
        public SkinnedMeshRenderer Renderer;
    }

    [SerializeField] private List<SlotBinding> _SlotBindings;
    private Dictionary<ItemType, SkinnedMeshRenderer> _RendererSlots;

    private const string PropKeyPrefix = "Customize_";

    void Awake()
    {
        // 슬롯 바인딩을 빠르게 조회할 딕셔너리 생성
        _RendererSlots = new Dictionary<ItemType, SkinnedMeshRenderer>();
        foreach (var binding in _SlotBindings)
        {
            if (!_RendererSlots.ContainsKey(binding.Type))
                _RendererSlots.Add(binding.Type, binding.Renderer);
        }
    }

    /// <summary>
    /// 아이템 장착 시 호출: Custom Properties에 업데이트
    /// </summary>
    public void EquipItem(CustomizeItemSO itemSO)
    {
        if (!photonView.IsMine) return;

        var type   = itemSO.Type;
        var itemId = itemSO.ID;

        // Custom Player Properties 갱신 → 방내 모든 클라이언트에 푸시
        var props = new Hashtable { { PropKeyPrefix + (int)type, itemId } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        // 내 화면에도 즉시 적용
        ApplyMesh(type, itemId);
    }

    /// <summary>
    /// 방 입장 시: 해당 뷰의 소유자 프로퍼티만 적용
    /// </summary>
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        // 모든 플레이어의 프로퍼티를 가져오지 않고, 이 PhotonView의 소유자만 처리
        var owner = photonView.Owner;
        if (owner != null)
        {
            ApplyAllProperties(owner.CustomProperties);
        }
    }

    /// <summary>
    /// 다른 플레이어의 Custom Properties 변경 시
    /// </summary>
    public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(target, changedProps);
        // 이 PhotonView의 소유자가 아니라면 무시
        if (target != photonView.Owner) return;

        ApplyAllProperties(changedProps);
    }

    /// <summary>
    /// Hashtable에 들어 있는 커스터마이징 키만 찾아 적용
    /// </summary>
    private void ApplyAllProperties(Hashtable props)
    {
        foreach (System.Collections.DictionaryEntry entry in props)
        {
            var key = entry.Key as string;
            if (string.IsNullOrEmpty(key) || !key.StartsWith(PropKeyPrefix))
                continue;

            if (int.TryParse(key.Substring(PropKeyPrefix.Length), out int typeInt))
            {
                var type   = (ItemType)typeInt;
                var itemId = entry.Value as string;
                ApplyMesh(type, itemId);
            }
        }
    }

    /// <summary>
    /// 실제로 Mesh를 교체하는 공통 로직
    /// </summary>
    private void ApplyMesh(ItemType type, string itemId)
    {
        if (!_RendererSlots.TryGetValue(type, out var renderer))
        {
            Debug.LogError($"Renderer가 없습니다: {type}");
            return;
        }

        var itemPool = CustomizeItemPoolLocator._Inst;
        if (itemPool == null)
        {
            Debug.LogError("CustomizeItemPoolSO가 등록되지 않았습니다.");
            return;
        }

        var item = itemPool.GetItems(type)
                           .FirstOrDefault(i => i.ID == itemId);
        if (item == null)
        {
            Debug.LogWarning($"ID '{itemId}' 아이템을 찾을 수 없습니다.");
            return;
        }

        renderer.sharedMesh = item.ItemMesh;
    }
}
