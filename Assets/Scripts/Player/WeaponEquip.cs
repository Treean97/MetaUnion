using System.Collections.Generic;
using Controller;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Animations.Rigging;

using Hashtable = ExitGames.Client.Photon.Hashtable;

[System.Serializable]
public class WeaponEntry
{
    public string Name;
    public GameObject Prefab;
    public Transform InstantiateTransform;
}

public class WeaponEquip : MonoBehaviourPunCallbacks
{
    private const string PROP_EQUIP = "equipWeapon";

    [Header("무기 리스트")]
    [SerializeField] private List<WeaponEntry> _Weapons;
    private Dictionary<string, WeaponEntry> _WeaponDict;

    [Header("무기 생성 부모")]
    [SerializeField] private Transform _HandAnchor;

    [Header("Rig & IK 세팅")]
    [SerializeField] private Rig _WeaponRig;
    [SerializeField] private TwoBoneIKConstraint _RightArmIK;
    [SerializeField] private TwoBoneIKConstraint _LeftArmIK;

    [Header("입력")]
    [SerializeField] private PlayerInput _Input;

    private GameObject _CurrentWeapon;

    void OnAxeKey() => EquipWeapon("Axe");
    void OnPickaxeKey() => EquipWeapon("Pickaxe");
    
    private void Awake()
    {
        // 리스트 → 딕셔너리
        _WeaponDict = new Dictionary<string, WeaponEntry>(_Weapons?.Count ?? 0);
        if (_Weapons != null)
        {
            foreach (var entry in _Weapons)
            {
                if (string.IsNullOrEmpty(entry.Name) || entry.Prefab == null) continue;
                if (_WeaponDict.ContainsKey(entry.Name))
                    Debug.LogWarning($"WeaponEquip: 중복된 무기 이름 '{entry.Name}'");
                else
                    _WeaponDict.Add(entry.Name, entry);
            }
        }

        // 로컬 소유자에게만 입력 바인딩
        if (_Input && photonView.IsMine)
        {
            _Input.OnSlot_0KeyPressed += UnequipWeapon;
            _Input.OnSlot_1KeyPressed += OnAxeKey;
            _Input.OnSlot_2KeyPressed += OnPickaxeKey;
        }
    }

    private void OnDestroy()
    {
        if (_Input && photonView.IsMine)
        {
            _Input.OnSlot_0KeyPressed    -= UnequipWeapon;
            _Input.OnSlot_1KeyPressed     -= OnAxeKey;
            _Input.OnSlot_2KeyPressed -= OnPickaxeKey;
        }
    }

    private void Start()
    {
        // 스폰 직후 소유자의 커스텀프로퍼티로 초기 상태 동기화 (로컬/원격 공통)
        SyncFromProperties(photonView.Owner);
    }

    public void EquipWeapon(string name)
    {
        if (!photonView.IsMine) return;

        if (!_WeaponDict.ContainsKey(name))
        {
            Debug.LogWarning($"WeaponEquip: 정의되지 않은 무기 '{name}'");
            return;
        }

        // 프로퍼티 갱신(전파)
        var table = new Hashtable { [PROP_EQUIP] = name };
        PhotonNetwork.LocalPlayer.SetCustomProperties(table);

        // 로컬 즉시 반영(콜백 대기 안 함)
        ApplyEquipLocally(name);
    }

    public void UnequipWeapon()
    {
        if (!photonView.IsMine) return;

        var table = new Hashtable { [PROP_EQUIP] = string.Empty };
        PhotonNetwork.LocalPlayer.SetCustomProperties(table);

        ApplyUnequipLocally();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        // 이 뷰의 소유자에 대한 변경만 처리
        if (targetPlayer != photonView.Owner) return;
        if (!changedProps.ContainsKey(PROP_EQUIP)) return;

        var name = changedProps[PROP_EQUIP] as string;
        if (string.IsNullOrEmpty(name)) ApplyUnequipLocally();
        else ApplyEquipLocally(name);
    }

    public override void OnJoinedRoom()
    {
        // 재입장/씬 재진입 대비하여 한 번 더 안전 동기화
        SyncFromProperties(photonView.Owner);
    }

    private void SyncFromProperties(Player owner)
    {
        if (owner == null) return;

        var props = owner.CustomProperties;
        if (props != null && props.TryGetValue(PROP_EQUIP, out var v) && v is string name && !string.IsNullOrEmpty(name))
            ApplyEquipLocally(name);
        else
            ApplyUnequipLocally();
    }

    private void ApplyEquipLocally(string name)
    {
        if (!_WeaponDict.TryGetValue(name, out var w))
        {
            Debug.LogWarning($"Weapon '{name}' not found");
            return;
        }

        if (_CurrentWeapon) Destroy(_CurrentWeapon);

        _CurrentWeapon = Instantiate(w.Prefab, _HandAnchor, false);

        if (w.InstantiateTransform)
        {
            _CurrentWeapon.transform.localPosition = w.InstantiateTransform.localPosition;
            _CurrentWeapon.transform.localRotation = w.InstantiateTransform.localRotation;
        }

        var leftGrip = _CurrentWeapon.transform.Find("LeftGrip");
        var rightGrip = _CurrentWeapon.transform.Find("RightGrip");

        if (!leftGrip || !rightGrip)
        {
            Debug.LogWarning("LeftGrip/RightGrip 없음");
            _LeftArmIK.weight = _RightArmIK.weight = _WeaponRig.weight = 0f;
            return;
        }

        var L = _LeftArmIK.data; L.target = leftGrip; _LeftArmIK.data = L;
        var R = _RightArmIK.data; R.target = rightGrip; _RightArmIK.data = R;

        _LeftArmIK.weight = 1f;
        _RightArmIK.weight = 1f;
        _WeaponRig.weight = 1f;
    }

    private void ApplyUnequipLocally()
    {
        if (_CurrentWeapon) { Destroy(_CurrentWeapon); _CurrentWeapon = null; }
        _LeftArmIK.weight = _RightArmIK.weight = _WeaponRig.weight = 0f;
    }

    // private void Awake()
    // {
    //     // 리스트 → 딕셔너리
    //     _WeaponDict = new Dictionary<string, WeaponEntry>();
    //     foreach (var entry in _Weapons)
    //     {
    //         if (string.IsNullOrEmpty(entry.Name) || entry.Prefab == null)
    //             continue;

    //         if (_WeaponDict.ContainsKey(entry.Name))
    //             Debug.LogWarning($"WeaponEquip: 중복된 무기 이름 '{entry.Name}'");
    //         else
    //             _WeaponDict.Add(entry.Name, entry);
    //     }

    // _Input.OnHandKeyPressed += UnequipWeapon;
    // _Input.OnAxeKeyPressed += OnAxeKey;
    // _Input.OnPickaxeKeyPressed += OnPickaxeKey;
    // }

    // void OnDestroy()
    // {
    //     _Input.OnHandKeyPressed -= UnequipWeapon;
    //     _Input.OnAxeKeyPressed -= OnAxeKey;
    //     _Input.OnPickaxeKeyPressed -= OnPickaxeKey;
    // }

    //     public void EquipWeapon(string name)
    // {
    //     // 내 캐릭터면 전파, 모두가 동일하게 로컬 생성
    //     if (photonView.IsMine)
    //         photonView.RPC(nameof(RPC_EquipWeapon), RpcTarget.All, name);
    // }

    // public void UnequipWeapon()
    // {
    //     if (photonView.IsMine)
    //         photonView.RPC(nameof(RPC_UnequipWeapon), RpcTarget.All);
    // }

    // [PunRPC] void RPC_EquipWeapon(string name)
    // {
    //     if (!_WeaponDict.TryGetValue(name, out var w))
    //     {
    //         Debug.LogWarning($"Weapon '{name}' not found");
    //         return;
    //     }

    //     // 기존 제거(로컬)
    //     if (_CurrentWeapon) Destroy(_CurrentWeapon);

    //     _CurrentWeapon = Instantiate(w.Prefab, _HandAnchor, false);
    //     _CurrentWeapon.transform.localPosition = w.InstantiateTransform.localPosition;
    //     _CurrentWeapon.transform.localRotation = w.InstantiateTransform.localRotation;

    //     // IK 타깃
    //     var leftGrip  = _CurrentWeapon.transform.Find("LeftGrip");
    //     var rightGrip = _CurrentWeapon.transform.Find("RightGrip");
    //     if (!leftGrip || !rightGrip) { Debug.LogWarning("LeftGrip/RightGrip 없음"); return; }

    //     var L = _LeftArmIK.data; L.target = leftGrip;   _LeftArmIK.data = L;
    //     var R = _RightArmIK.data; R.target = rightGrip; _RightArmIK.data = R;

    //     _LeftArmIK.weight  = 1f;
    //     _RightArmIK.weight = 1f;
    //     _WeaponRig.weight  = 1f;
    // }

    // [PunRPC] void RPC_UnequipWeapon()
    // {
    //     if (_CurrentWeapon) { Destroy(_CurrentWeapon); _CurrentWeapon = null; }
    //     _LeftArmIK.weight = _RightArmIK.weight = _WeaponRig.weight = 0f;
    // }
}
