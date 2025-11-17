using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;

public class PlayerPreviewManager : MonoBehaviourPunCallbacks
{
    [Serializable]
    public class SlotBinding
    {
        public ItemType Type;
        public SkinnedMeshRenderer MeshRenderer; // 프리뷰 쪽 렌더러
        public Mesh BaseMesh;
    }

    [SerializeField]
    private List<SlotBinding> _SlotBindings;

    private Dictionary<ItemType, SkinnedMeshRenderer> _RendererSlots;

    private const string PropKeyPrefix = "Customize_";
    private const string UnEquipToken  = "0";

    [Header("Color Settings")]
    [SerializeField] string _ColorProperty = "_BaseColor";

    void Awake()
    {
        // 슬롯 매핑 빌드
        _RendererSlots = new Dictionary<ItemType, SkinnedMeshRenderer>();
        foreach (var binding in _SlotBindings)
        {
            if (binding.MeshRenderer && !_RendererSlots.ContainsKey(binding.Type))
                _RendererSlots.Add(binding.Type, binding.MeshRenderer);
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        GameEvents.OnRequestPreviewItemColor += HandlePreviewItemColor;
    }

    public override void OnDisable()
    {
        GameEvents.OnRequestPreviewItemColor -= HandlePreviewItemColor;
        base.OnDisable();
    }

    void Start()
    {
        // 시작 시 로컬 플레이어의 기존 프로퍼티 즉시 반영
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
        // 렌더러 조회
        if (!_RendererSlots.TryGetValue(type, out var renderer))
        {
            Debug.LogError($"Renderer가 없습니다: {type}");
            return;
        }

        // 타입을 통해 객체 찾기
        var binding = _SlotBindings.FirstOrDefault(b => b.Type == type);
        if (binding == null) return;

        // 해제 처리
        if (string.IsNullOrEmpty(itemId) || itemId == UnEquipToken || itemId == "-1")
        {
            renderer.sharedMesh = binding.BaseMesh;
            return;
        }

        // 적용 처리
        var itemSO = ItemManager._Inst.GetCustomizeItem(itemId);
        if (itemSO == null)
        {
            Debug.LogWarning($"ID '{itemId}' 아이템을 찾을 수 없습니다.");
            return;
        }

        // Mesh 교체
        renderer.sharedMesh = itemSO.ItemMesh;
    }

    private void HandlePreviewItemColor(CustomizeItemSO item, Color color)
    {
        var type = item.Type;

        // 해당 타입의 프리뷰 렌더러 찾기
        if (!_RendererSlots.TryGetValue(type, out var renderer) || renderer == null)
            return;

        var mats = renderer.materials; // 프리뷰는 인스턴스 머티리얼 써도 됨

        for (int i = 0; i < mats.Length; i++)
        {
            var mat = mats[i];
            if (mat != null && mat.HasProperty(_ColorProperty))
            {
                mat.SetColor(_ColorProperty, color);
            }
        }

        renderer.materials = mats;
    }
}
