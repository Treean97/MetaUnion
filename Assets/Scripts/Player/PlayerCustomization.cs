using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerCustomization : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback, ICloudSaveSection
{
    [Serializable]
    public class SlotBinding
    {
        public ItemType Type;
        public SkinnedMeshRenderer MeshRenderer;
        public Mesh BaseMesh;
    }

    [SerializeField] 
    private List<SlotBinding> _SlotBindings;

    // 타입별 렌더러를 빠르게 찾기 위한 딕셔너리
    private Dictionary<ItemType, SkinnedMeshRenderer> _RendererSlots;

    // 프로퍼티 키 접두사
    private const string PropKeyPrefix = "Customize_";
    private const string ColorPropKeyPrefix = "CustomizeColor_";
    private const string UnEquipToken  = "0";

    public string Key => "customize";
    private Dictionary<ItemType, string> _Equipped = new();

    [Header("Color Settings")]
    [SerializeField] string _ColorProperty = "_BaseColor";
    private readonly Dictionary<ItemType, Color> _Colors = new();


    // 저장용 DTO
    public class CustomizeSettingsDTO
    {
        [Serializable]
        public class Entry 
        { 
            public int Type; 
            public string Id; 
            public Color Color;
        }

        public List<Entry> Items = new();
    }


    void Awake()
    {
        // Awake 시점에 슬롯 바인딩을 딕셔너리로 빌드
        _RendererSlots = new Dictionary<ItemType, SkinnedMeshRenderer>();

        foreach (var binding in _SlotBindings)
        {
            if (binding.MeshRenderer && !_RendererSlots.ContainsKey(binding.Type))
                _RendererSlots.Add(binding.Type, binding.MeshRenderer);

            // 기본 메쉬 캐싱
            if (binding.MeshRenderer)
                binding.BaseMesh = binding.MeshRenderer.sharedMesh;
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();  // Photon 콜백 등록 유지
        GameEvents.OnRequestApplyItemColor += HandleApplyItemColor;
    }

    public override void OnDisable()
    {
        GameEvents.OnRequestApplyItemColor -= HandleApplyItemColor;
        base.OnDisable(); // Photon 콜백 등록 해제
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            SaveLoadManager._Inst?.RegisterCloud(this);
        }
        
    }

    void OnDestroy()
    {
        if (photonView.IsMine)
        {
            SaveLoadManager._Inst?.UnregisterCloud(this);
        }
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

        var type = itemSO.Type;
        var itemId = itemSO.ID;

        // 방 전체에 변경된 프로퍼티 전파
        var props = new Hashtable { { PropKeyPrefix + (int)type, itemId } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        // 로컬 화면에 즉시 반영
        ApplyMesh(type, itemId);
        
        // 저장
        SaveLoadManager._Inst?.SaveCloudSection(Key);
    }

    public void UnEquipItem(ItemType type)
    {
        if (!photonView.IsMine) return;

        // 방 전체에 변경된 프로퍼티 전파
        var props = new Hashtable { { PropKeyPrefix + (int)type, UnEquipToken } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        // 로컬 화면에 즉시 반영
        ApplyMesh(type, UnEquipToken);
        
        // 저장
        SaveLoadManager._Inst?.SaveCloudSection(Key);
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
    /// 전달된 Hashtable에서 프로퍼티 키를 찾아 적용
    /// </summary>
    private void ApplyAllProperties(Hashtable props)
    {
        foreach (System.Collections.DictionaryEntry entry in props)
        {
            var key = entry.Key as string;
            if (string.IsNullOrEmpty(key)) 
                continue;

            // 장착 상태
            if (key.StartsWith(PropKeyPrefix))
            {
                if (int.TryParse(key.Substring(PropKeyPrefix.Length), out int typeInt))
                {
                    var type   = (ItemType)typeInt;
                    var itemId = entry.Value?.ToString();

                    ApplyMesh(type, itemId);
                }

                continue;
            }

            // 색상 상태
            if (key.StartsWith(ColorPropKeyPrefix))
            {
                if (int.TryParse(key.Substring(ColorPropKeyPrefix.Length), out int typeInt))
                {
                    var type = (ItemType)typeInt;

                    // 값은 "RRGGBBAA" 문자열 or null
                    var hex = entry.Value as string;
                    if (!string.IsNullOrEmpty(hex))
                    {
                        // ColorUtility는 "#RRGGBBAA" 형태를 기대하므로 # 붙여줌
                        if (ColorUtility.TryParseHtmlString("#" + hex, out var color))
                        {
                            _Colors[type] = color;
                            ApplyColor(type, color);
                        }
                    }
                }

                continue;
            }
        }
    }

    /// <summary>
    /// 실제로 SkinnedMeshRenderer.sharedMesh를 변경해주는 공통 로직
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
            if (photonView.IsMine) _Equipped[type] = UnEquipToken;
            return;
        }

        // 적용 처리
        var itemSO = ItemManager._Inst.GetCustomizeItem(itemId);
        if (itemSO == null)
        {
            Debug.LogWarning($"ID '{itemId}' 아이템을 찾을 수 없습니다.");
            renderer.sharedMesh = binding.BaseMesh;
            if (photonView.IsMine) _Equipped[type] = UnEquipToken;
            return;
        }

        // Mesh 교체
        renderer.sharedMesh = itemSO.ItemMesh;
        _Equipped[type] = itemId;
    }

    private void ApplyColor(ItemType type, Color color)
    {
        if (!_RendererSlots.TryGetValue(type, out var renderer) || renderer == null)
            return;

        // 필요하면 특정 머티리얼 인덱스만 변경하도록 바꿀 수 있음
        var mats = renderer.materials; // 인스턴스 머티리얼

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

    private void HandleApplyItemColor(CustomizeItemSO item, Color color)
    {
        if (!photonView.IsMine) return;

        var type = item.Type;

        // 로컬 상태/머티리얼 적용
        _Colors[type] = color;
        ApplyColor(type, color);

        // 멀티 전파: Photon Player CustomProperties에 색상을 기록
        // "#RRGGBBAA" 형태의 문자열로 저장
        string hex = ColorUtility.ToHtmlStringRGBA(color);

        var props = new Hashtable
        {
            { ColorPropKeyPrefix + (int)type, hex }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        // 클라우드 저장
        SaveLoadManager._Inst?.SaveCloudSection(Key);
    }

    public string CaptureJson()
    {
        var dto = new CustomizeSettingsDTO();

        foreach (var b in _SlotBindings)
        {
            var type = b.Type;

            // 장착 ID 결정
            var id = UnEquipToken;
            if (_Equipped.TryGetValue(type, out var cur))
            {
                id = string.IsNullOrEmpty(cur) ? UnEquipToken : cur;
            }
            else
            {
                if (PhotonNetwork.LocalPlayer != null)
                {
                    var key = PropKeyPrefix + (int)type;
                    if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(key, out var v) && v != null)
                        id = v.ToString();
                }
            }

            // 색 결정
            Color color = Color.white;
            if (_Colors.TryGetValue(type, out var savedColor))
                color = savedColor;

            dto.Items.Add(new CustomizeSettingsDTO.Entry
            {
                Type  = (int)type,
                Id    = id,
                Color = color
            });
        }

        return JsonUtility.ToJson(dto);
    }


    public void ApplyJson(string s)
    {
        // Unity fake-null 체크 : 이미 Destroy된 컴포넌트면 리턴
        if (!this) return;
        if (!photonView.IsMine) return;
        if (string.IsNullOrEmpty(s)) return;

        CustomizeSettingsDTO dto = null;
        try { dto = JsonUtility.FromJson<CustomizeSettingsDTO>(s); } catch { }
        if (dto?.Items == null) return;

        foreach (var e in dto.Items)
        {
            var type  = (ItemType)e.Type;
            var id    = string.IsNullOrEmpty(e.Id) ? UnEquipToken : e.Id;
            var color = e.Color;

            if (id == UnEquipToken || id == "-1")
            {
                UnEquipItem(type);
                _Colors.Remove(type);
                continue;
            }

            var so = ItemManager._Inst.GetCustomizeItem(id);
            if (so == null)
            {
                UnEquipItem(type);
                _Colors.Remove(type);
                continue;
            }

            // 메쉬 장착
            EquipItem(so);

            // 색 복원 (구버전 데이터 방어)
            // Color 기본값(0,0,0,0)은 "색 정보 없음"으로 간주
            bool hasColorData =
                color.r != 0f || color.g != 0f || color.b != 0f || color.a != 0f;

            if (hasColorData)
            {
                _Colors[type] = color;
                ApplyColor(type, color);

                if (PhotonNetwork.LocalPlayer != null)
                {
                    string hex = ColorUtility.ToHtmlStringRGBA(color);
                    var props = new Hashtable
                    {
                        { ColorPropKeyPrefix + (int)type, hex }
                    };
                    PhotonNetwork.LocalPlayer.SetCustomProperties(props);
                }
            }
            else
            {
                // 색 정보가 없으면 머티리얼 원래 색 유지
                _Colors.Remove(type);
            }
        }
    }

}
