using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CustomizeCamera : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _CineCamera;
    [SerializeField] private float _YawSpeedDegPerSec = 120f; // 초당 회전 각도

    private CinemachineOrbitalFollow _Orbital;
    private bool _LeftHeld, _RightHeld;

    [Serializable]
    public class TargetBinding
    {
        public ItemType Type;       // 예: Hair, Face, Body, Legs ...
        public Transform Target;    // 해당 타입을 볼 때의 Pivot/LookAt 기준점
    }

    [SerializeField] private CustomizeCamera _CustomizeCamera;   // 같은 프리팹에 있음
    [SerializeField] private List<TargetBinding> _TargetBindings;      // 인스펙터에서 타입-타겟 매핑

    private Dictionary<ItemType, Transform> _TargetDic;

    private ItemType _CurType;

    void Awake()
    {
        _Orbital = _CineCamera ? _CineCamera.GetComponent<CinemachineOrbitalFollow>() : null;
        _CurType = ItemType.Hair;

        _TargetDic = new Dictionary<ItemType, Transform>();
        foreach (var binding in _TargetBindings)
        {
            if (binding.Target && !_TargetDic.ContainsKey(binding.Type))
                _TargetDic.Add(binding.Type, binding.Target);
        }
    }

    void Update()
    {
        if (_Orbital == null) return;

        int dir = (_RightHeld ? 1 : 0) + (_LeftHeld ? -1 : 0);
        if (dir != 0)
        {
            _Orbital.HorizontalAxis.Value += dir * _YawSpeedDegPerSec * Time.unscaledDeltaTime;
        }
    }

    // 버튼 이벤트(홀드 회전)
    public void OnLeftDown() { _LeftHeld = true; }
    public void OnLeftUp() { _LeftHeld = false; }
    public void OnRightDown() { _RightHeld = true; }
    public void OnRightUp() { _RightHeld = false; }

    // 클릭 한 번에 일정 각도 회전
    public void StepLeft(float deg = 15f)
    {
        if (_Orbital == null) return;
        _Orbital.HorizontalAxis.Value -= deg;
    }
    public void StepRight(float deg = 15f)
    {
        if (_Orbital == null) return;
        _Orbital.HorizontalAxis.Value += deg;
    }

    // 타겟 교체
    public void SetSingleTarget(ItemType type)
    {
        if (!_TargetDic.TryGetValue(type, out Transform t) || !t)
        {
            Debug.LogWarning($"타겟 없음: {type}");
            return;
        }

        var target = _CineCamera.Target;   // 구조체 복사
        target.TrackingTarget = t;         // Follow
        target.CustomLookAtTarget = false; // LookAt=Follow
        target.LookAtTarget = null;
        _CineCamera.Target = target;       // 구조체 다시 대입 (필수)    
    }
}
