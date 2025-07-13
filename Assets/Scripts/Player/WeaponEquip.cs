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

public class WeaponEquip : MonoBehaviour
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
        _Input.OnAxeKeyPressed += () => EquipWeapon("Axe");
        _Input.OnPickaxeKeyPressed += () => EquipWeapon("Pickaxe");
    }

    public void EquipWeapon(string weaponName)
    {
        if (!_WeaponDict.TryGetValue(weaponName, out var weapon))
        {
            Debug.LogError($"WeaponEquip: '{weaponName}' 무기를 찾을 수 없습니다.");
            return;
        }

        // 이전 무기 제거
        if (_CurrentWeapon != null)
            PhotonNetwork.Destroy(_CurrentWeapon);

        // 새 무기 인스턴스화 
        var weaponInstance = PhotonNetwork.Instantiate(
            weapon.Prefab.name,
            Vector3.zero,
            Quaternion.identity,
            0
        );

        weaponInstance.transform.SetParent(_HandAnchor, false);
        weaponInstance.transform.localPosition = weapon.InstantiateTransform.localPosition;
        weaponInstance.transform.localRotation = weapon.InstantiateTransform.localRotation;
        _CurrentWeapon = weaponInstance;

        // Grip 찾기
        var leftGrip  = weaponInstance.transform.Find("LeftGrip");
        var rightGrip = weaponInstance.transform.Find("RightGrip");
        if (leftGrip == null || rightGrip == null)
        {
            Debug.LogError("WeaponEquip: Prefab에 'LeftGrip' 또는 'RightGrip' Transform이 없습니다.");
            return;
        }

        // IK Constraint.data 수정 후 재할당
        var leftData = _LeftArmIK.data;
        leftData.target = leftGrip;
        _LeftArmIK.data = leftData;

        var rightData = _RightArmIK.data;
        rightData.target = rightGrip;
        _RightArmIK.data = rightData;

        // IK & Rig 활성화
        _LeftArmIK.weight  = 1f;
        _RightArmIK.weight = 1f;
        _WeaponRig.weight  = 1f;
    }

    public void UnequipWeapon()
    {
        if (_CurrentWeapon != null)
        {
            Destroy(_CurrentWeapon);
            _CurrentWeapon = null;
        }

        _LeftArmIK.weight  = 0f;
        _RightArmIK.weight = 0f;
        _WeaponRig.weight  = 0f;
    }
}
