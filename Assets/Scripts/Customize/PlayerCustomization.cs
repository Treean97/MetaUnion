using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerCustomization : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    [Serializable]
    public class SlotBinding
    {
        public ItemType Type;
        public SkinnedMeshRenderer Renderer;
    }

    [SerializeField] 
    private List<SlotBinding> _SlotBindings;

    // 타입별 렌더러를 빠르게 찾기 위한 딕셔너리
    private Dictionary<ItemType, SkinnedMeshRenderer> _RendererSlots;

    // 프로퍼티 키 접두사
    private const string PropKeyPrefix = "Customize_";

    void Awake()
    {
        // Awake 시점에 슬롯 바인딩을 딕셔너리로 빌드
        _RendererSlots = new Dictionary<ItemType, SkinnedMeshRenderer>();
        foreach (var binding in _SlotBindings)
        {
            if (!_RendererSlots.ContainsKey(binding.Type))
                _RendererSlots.Add(binding.Type, binding.Renderer);
        }
    }

    void Start()
    {
        // 이 오브젝트가 활성화된 직후, 이미 세팅된 CustomProperties가 있으면 바로 적용
        if (photonView.Owner != null)
            ApplyAllProperties(photonView.Owner.CustomProperties);
    }

    // Photon이 이 프리팹을 인스턴스화할 때 호출 (Instantiate 시점)
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (photonView.Owner != null)
            ApplyAllProperties(photonView.Owner.CustomProperties);
    }

    /// <summary>
    /// 로컬 플레이어가 아이템을 장착할 때 호출
    /// → 커스텀 프로퍼티를 갱신하고, 로컬에도 즉시 적용
    /// </summary>
    public void EquipItem(CustomizeItemSO itemSO)
    {
        if (!photonView.IsMine) return;

        var type   = itemSO.Type;
        var itemId = itemSO.ID;

        // 1) 방 전체에 변경된 프로퍼티 전파
        var props = new Hashtable { { PropKeyPrefix + (int)type, itemId } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        // 2) 로컬 화면에 즉시 반영
        ApplyMesh(type, itemId);
    }

    /// <summary>
    /// 로컬 클라이언트가 룸에 입장했을 때 호출
    /// → 기존에 세팅된 프로퍼티를 한 번 더 적용
    /// </summary>
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        var owner = photonView.Owner;
        if (owner != null)
            ApplyAllProperties(owner.CustomProperties);
    }

    /// <summary>
    /// 다른 플레이어(A)의 CustomProperties가 갱신되었을 때 호출
    /// → 자신이 소유하지 않은 뷰에서만 반응
    /// </summary>
    public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(target, changedProps);

        // 이 뷰의 소유자가 아닌 경우 무시
        if (target != photonView.Owner) return;

        ApplyAllProperties(changedProps);
    }

    /// <summary>
    /// 전달된 Hashtable에서 "Customize_" 키를 찾아 
    /// 각 슬롯에 해당하는 메시 호출
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
    /// 실제로 SkinnedMeshRenderer.sharedMesh를 변경해주는 공통 로직
    /// </summary>
    private void ApplyMesh(ItemType type, string itemId)
    {
        // 1) 렌더러 조회
        if (!_RendererSlots.TryGetValue(type, out var renderer))
        {
            Debug.LogError($"Renderer가 없습니다: {type}");
            return;
        }

        // 2) ItemManager에서 아이템 정보 조회
        var itemSO = ItemManager._Inst.GetItem(type, itemId);
        if (itemSO == null)
        {
            Debug.LogWarning($"ID '{itemId}' 아이템을 찾을 수 없습니다.");
            return;
        }

        // 3) Mesh 교체
        renderer.sharedMesh = itemSO.ItemMesh;
    }

}
