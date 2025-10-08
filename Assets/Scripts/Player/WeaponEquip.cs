using System.Collections;
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

    [Header("IK 조절")]
    [Range(0, 1)] public float IdleIKWeight = 0.15f;
    [Range(0, 1)] public float HoldIKWeight = 0.2f;
    [Range(0, 1)] public float AttackIKWeight = 1.00f;
    [Range(0, 1)] public float UnequipIKWeight = 0f;
    [Min(0f)] public float BlendTime = 0.1f;

    [Header("입력")]
    [SerializeField] private PlayerInput _Input;

    [Header("문자열")]
    [SerializeField] private string _AxeName = "Axe";
    [SerializeField] private string _PickAxeName = "Pickaxe";
    [SerializeField] private string _FishingRodName = "FishingRod";

    AttackHandler _AttackHandler;

    private GameObject _CurrentWeapon;
    private Coroutine _IKBlend;

    private bool _HasWeapon;

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

            FishingSequence.OnFishingStart += HandleFishingStart;
            FishingSequence.OnFishingEnd += HandleFishingEnd;
        }

        _AttackHandler = GetComponent<AttackHandler>();
        if (_AttackHandler)
        {
            _AttackHandler.OnAttackStart += OnAttackStart_IK;
            _AttackHandler.OnAttackEnd += OnAttackEnd_IK;
        }

        // 시작 웨이트 0으로
        ApplyIKImmediate(0f);
        _HasWeapon = false;
    }

    private void OnDestroy()
    {
        if (_Input && photonView.IsMine)
        {
            _Input.OnSlot_0KeyPressed -= UnequipWeapon;
            _Input.OnSlot_1KeyPressed -= OnAxeKey;
            _Input.OnSlot_2KeyPressed -= OnPickaxeKey;
        }

        if (_AttackHandler)
        {
            _AttackHandler.OnAttackStart -= OnAttackStart_IK;
            _AttackHandler.OnAttackEnd -= OnAttackEnd_IK;
        }

        FishingSequence.OnFishingStart -= HandleFishingStart;
        FishingSequence.OnFishingEnd -= HandleFishingEnd;
    }

    // === 1번 적용: 공격 이벤트에서도 무기 없으면 블렌딩 금지 ===
    void OnAttackStart_IK() { if (_HasWeapon) BlendIKTo(AttackIKWeight, BlendTime); }
    void OnAttackEnd_IK()   { if (_HasWeapon) BlendIKTo(IdleIKWeight,   BlendTime); }

    void BlendIKTo(float target, float time)
    {
        // === 1번 적용: 무기 없으면 블렌딩 진입 자체를 막음 ===
        if (!_HasWeapon) return;

        if (_IKBlend != null) StopCoroutine(_IKBlend);
        _IKBlend = StartCoroutine(CoBlendIK(target, Mathf.Max(0.0001f, time)));
    }

    IEnumerator CoBlendIK(float target, float time)
    {
        float start = _WeaponRig ? _WeaponRig.weight : 0f;
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            float w = Mathf.Lerp(start, target, t / time);
            ApplyIKImmediate(w);
            yield return null;
        }
        ApplyIKImmediate(target);
        _IKBlend = null;
    }

    void ApplyIKImmediate(float w)
    {
        if (_WeaponRig) _WeaponRig.weight = w;
        if (_LeftArmIK) _LeftArmIK.weight = w;
        if (_RightArmIK) _RightArmIK.weight = w;
    }

    public void EquipWeapon(string name)
    {
        if (!photonView.IsMine) return;

        if (!_WeaponDict.ContainsKey(name))
        {
            Debug.LogWarning($"WeaponEquip: 정의되지 않은 무기 '{name}'");
            return;
        }

        var table = new Hashtable { [PROP_EQUIP] = name };
        PhotonNetwork.LocalPlayer.SetCustomProperties(table);

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
        if (targetPlayer != photonView.Owner) return;
        if (!changedProps.ContainsKey(PROP_EQUIP)) return;

        var name = changedProps[PROP_EQUIP] as string;
        if (string.IsNullOrEmpty(name)) ApplyUnequipLocally();
        else ApplyEquipLocally(name);
    }

    public override void OnJoinedRoom()
    {
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
            _HasWeapon = true; // 그립 없지만 장착 상태는 true로 표시(필요 시 false로 변경 가능)
            return;
        }

        var L = _LeftArmIK.data; L.target = leftGrip; _LeftArmIK.data = L;
        var R = _RightArmIK.data; R.target = rightGrip; _RightArmIK.data = R;

        if (_IKBlend != null) StopCoroutine(_IKBlend);
        _HasWeapon = true;                    // === 1번 적용
        ApplyIKImmediate(IdleIKWeight);
    }

    private void ApplyUnequipLocally()
    {
        if (_CurrentWeapon) { Destroy(_CurrentWeapon); _CurrentWeapon = null; }

        _HasWeapon = false;                   // === 1번 적용
        if (_IKBlend != null) StopCoroutine(_IKBlend); // 진행 중 블렌딩 중단

        // 안전: 타겟 해제(선택)
        var L = _LeftArmIK.data;  L.target = null; _LeftArmIK.data  = L;
        var R = _RightArmIK.data; R.target = null; _RightArmIK.data = R;

        ApplyIKImmediate(UnequipIKWeight);    // 보통 0
    }

    #region "바인딩"
    void OnAxeKey() => EquipWeapon(_AxeName);
    void OnPickaxeKey() => EquipWeapon(_PickAxeName);

    void HandleFishingStart()
    {
        EquipWeapon(_FishingRodName);
        BlendIKTo(HoldIKWeight, BlendTime);
    }

    void HandleFishingEnd()
    {
        BlendIKTo(0f, BlendTime);
        UnequipWeapon();
    }
    #endregion
}
