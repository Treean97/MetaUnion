using System.Collections.Generic;
using Controller;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[System.Serializable]
public struct WeaponEntry
{
    [Tooltip("무기를 선택할 때 사용할 이름입니다.")]
    public string Name;
    [Tooltip("해당 이름에 맵핑할 무기 Prefab")]
    public GameObject Prefab;
}

public class WeaponEquip : MonoBehaviour
{
    [Header("무기 리스트")]
    [SerializeField] private List<WeaponEntry> _Weapons;
    private Dictionary<string, GameObject> _WeaponDict;

    [Header("Rig & IK 세팅")]
    [SerializeField] private Rig                _WeaponRig;  // WeaponRig에 붙은 Rig 컴포넌트
    [SerializeField] private TwoBoneIKConstraint _RightArmIK; // RightArmIK 오브젝트의 Two Bone IK Constraint
    [SerializeField] private TwoBoneIKConstraint _LeftArmIK;  // LeftArmIK 오브젝트의 Two Bone IK Constraint
    [SerializeField] private PlayerInput _Input;
    private GameObject _CurrentWeapon;

    private void Awake()
    {
        // 리스트 → 딕셔너리로 빌드
        _WeaponDict = new Dictionary<string, GameObject>();
        foreach (var entry in _Weapons)
        {
            if (string.IsNullOrEmpty(entry.Name) || entry.Prefab == null)
                continue;

            if (_WeaponDict.ContainsKey(entry.Name))
                Debug.LogWarning($"WeaponEquip: 중복된 무기 이름 '{entry.Name}' 감지");
            else
                _WeaponDict.Add(entry.Name, entry.Prefab);
        }

        _Input.OnHandKeyPressed += UnequipWeapon;
        _Input.OnAxeKeyPressed += () => EquipWeapon("Axe");
        _Input.OnPickaxeKeyPressed += () =>  EquipWeapon("Pickaxe");

    }


    /// <summary>
    /// <paramref name="weaponName"/>에 해당하는 프리팹을 장착하고 IK/리깅 활성화
    /// </summary>
    public void EquipWeapon(string weaponName)
    {
        if (!_WeaponDict.TryGetValue(weaponName, out var prefab))
        {
            Debug.LogError($"WeaponEquip: '{weaponName}' 무기를 찾을 수 없습니다.");
            return;
        }

        // 기존 무기 비활성
        if (_CurrentWeapon != null)
            _CurrentWeapon.gameObject.SetActive(false);

        // 새로운 무기 활성
        _CurrentWeapon = prefab;
        _CurrentWeapon.gameObject.SetActive(true);
        

        // 프리팹 내부에 'LeftGrip', 'RightGrip' 이름의 Transform이 있어야 합니다.
        var leftGrip = _CurrentWeapon.transform.Find("LeftGrip");
        var rightGrip = _CurrentWeapon.transform.Find("RightGrip");

        if (leftGrip == null || rightGrip == null)
        {
            Debug.LogError("WeaponEquip: Prefab에 'LeftGrip' 또는 'RightGrip' Transform이 없습니다.");
        }
        else
        {
            // IK 타겟에 할당
            _LeftArmIK.data.target = leftGrip;
            _RightArmIK.data.target = rightGrip;
        }

        // IK, Rig 활성화
        _LeftArmIK.weight = 1f;
        _RightArmIK.weight = 1f;
        _WeaponRig.weight = 1f;
    }

    /// <summary>
    /// 장착 해제하고 IK/리깅 비활성화
    /// </summary>
    public void UnequipWeapon()
    {
        if (_CurrentWeapon != null)
        {
            _CurrentWeapon.gameObject.SetActive(false);
            _CurrentWeapon = null;
        }

        _LeftArmIK.weight  = 0f;
        _RightArmIK.weight = 0f;
        _WeaponRig.weight  = 0f;
    }
}
