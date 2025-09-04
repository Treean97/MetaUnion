using System.Collections.Generic;
using Controller;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[System.Serializable]
public class WeaponEntry
{
    public string Name;
    public GameObject Prefab;
    public Transform InstantiateTransform;
}

public class WeaponEquip : MonoBehaviourPun
{
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
    
    void OnAxeKey()     => EquipWeapon("Axe");
    void OnPickaxeKey() => EquipWeapon("Pickaxe");

    private void Awake()
    {
        // 리스트 → 딕셔너리
        _WeaponDict = new Dictionary<string, WeaponEntry>();
        foreach (var entry in _Weapons)
        {
            if (string.IsNullOrEmpty(entry.Name) || entry.Prefab == null)
                continue;

            if (_WeaponDict.ContainsKey(entry.Name))
                Debug.LogWarning($"WeaponEquip: 중복된 무기 이름 '{entry.Name}'");
            else
                _WeaponDict.Add(entry.Name, entry);
        }

    _Input.OnHandKeyPressed += UnequipWeapon;
    _Input.OnAxeKeyPressed += OnAxeKey;
    _Input.OnPickaxeKeyPressed += OnPickaxeKey;
    }

    void OnDestroy()
    {
        _Input.OnHandKeyPressed -= UnequipWeapon;
        _Input.OnAxeKeyPressed -= OnAxeKey;
        _Input.OnPickaxeKeyPressed -= OnPickaxeKey;
    }

        public void EquipWeapon(string name)
    {
        // 내 캐릭터면 전파, 모두가 동일하게 로컬 생성
        if (photonView.IsMine)
            photonView.RPC(nameof(RPC_EquipWeapon), RpcTarget.All, name);
    }

    public void UnequipWeapon()
    {
        if (photonView.IsMine)
            photonView.RPC(nameof(RPC_UnequipWeapon), RpcTarget.All);
    }

    [PunRPC] void RPC_EquipWeapon(string name)
    {
        if (!_WeaponDict.TryGetValue(name, out var w))
        {
            Debug.LogWarning($"Weapon '{name}' not found");
            return;
        }

        // 기존 제거(로컬)
        if (_CurrentWeapon) Destroy(_CurrentWeapon);

        _CurrentWeapon = Instantiate(w.Prefab, _HandAnchor, false);
        _CurrentWeapon.transform.localPosition = w.InstantiateTransform.localPosition;
        _CurrentWeapon.transform.localRotation = w.InstantiateTransform.localRotation;

        // IK 타깃
        var leftGrip  = _CurrentWeapon.transform.Find("LeftGrip");
        var rightGrip = _CurrentWeapon.transform.Find("RightGrip");
        if (!leftGrip || !rightGrip) { Debug.LogWarning("LeftGrip/RightGrip 없음"); return; }

        var L = _LeftArmIK.data; L.target = leftGrip;   _LeftArmIK.data = L;
        var R = _RightArmIK.data; R.target = rightGrip; _RightArmIK.data = R;

        _LeftArmIK.weight  = 1f;
        _RightArmIK.weight = 1f;
        _WeaponRig.weight  = 1f;
    }

    [PunRPC] void RPC_UnequipWeapon()
    {
        if (_CurrentWeapon) { Destroy(_CurrentWeapon); _CurrentWeapon = null; }
        _LeftArmIK.weight = _RightArmIK.weight = _WeaponRig.weight = 0f;
    }
}
