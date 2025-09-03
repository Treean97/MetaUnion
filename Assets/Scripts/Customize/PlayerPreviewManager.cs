using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerPreviewManager : MonoBehaviourPunCallbacks
{
    [Serializable]
    public class SlotBinding
    {
        public ItemType Type;
        public SkinnedMeshRenderer Renderer; // 프리뷰 쪽 렌더러
    }

    [SerializeField]
    private List<SlotBinding> _SlotBindings;

    private Dictionary<ItemType, SkinnedMeshRenderer> _RendererSlots;

    private const string PropKeyPrefix = "Customize_";

    void Awake()
    {
        // 슬롯 매핑 빌드
        _RendererSlots = new Dictionary<ItemType, SkinnedMeshRenderer>();
        foreach (var binding in _SlotBindings)
        {
            if (binding.Renderer && !_RendererSlots.ContainsKey(binding.Type))
                _RendererSlots.Add(binding.Type, binding.Renderer);
        }
    }

    void Start()
    {
        // 시작 시 로컬 플레이어의 기존 프로퍼티 즉시 반영
        var lp = PhotonNetwork.LocalPlayer;
        if (lp != null)
            ApplyAllProperties(lp.CustomProperties);
    }

    /// <summary>
    /// 외부에서 수동으로 전체 재적용하고 싶을 때 호출
    /// </summary>
    public void RefreshAll()
    {
        var lp = PhotonNetwork.LocalPlayer;
        if (lp != null)
            ApplyAllProperties(lp.CustomProperties);            
    }

    /// <summary>
    /// 로컬 플레이어의 커스텀 프로퍼티가 바뀔 때마다 호출되어 변경분만 적용
    /// </summary>
    public override void OnPlayerPropertiesUpdate(Player target, PhotonHashtable changedProps)
    {
        if (target != PhotonNetwork.LocalPlayer) return;
        ApplyAllProperties(changedProps);
    }

    /// <summary>
    /// 전달된 Hashtable에서 "Customize_" 키를 찾아 해당 슬롯에 메쉬 적용
    /// </summary>
    private void ApplyAllProperties(PhotonHashtable props)
    {
        if (props == null) return;

        foreach (System.Collections.DictionaryEntry entry in props)
        {
            var key = entry.Key as string;
            if (string.IsNullOrEmpty(key) || !key.StartsWith(PropKeyPrefix))
                continue;

            if (!int.TryParse(key.Substring(PropKeyPrefix.Length), out int typeInt))
                continue;

            var type = (ItemType)typeInt;
            var itemId = entry.Value as string;
            ApplyMesh(type, itemId);
        }
    }

    /// <summary>
    /// 메쉬만(sharedMesh) 교체. 머티리얼은 변경하지 않음.
    /// </summary>
    private void ApplyMesh(ItemType type, string itemId)
    {
        if (!_RendererSlots.TryGetValue(type, out var renderer) || renderer == null)
        {
            Debug.LogWarning($"[Preview] Renderer가 없습니다: {type}");
            return;
        }

        var itemSO = ItemManager._Inst ? ItemManager._Inst.GetCustomizeItem(type, itemId) : null;
        if (itemSO == null || !itemSO.ItemMesh)
        {
            Debug.LogWarning($"[Preview] 아이템/메쉬 없음: {type}/{itemId}");
            return;
        }

        renderer.sharedMesh = itemSO.ItemMesh; // ← 메쉬만 교체
    }
}
