using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NPCAnimationController : MonoBehaviour
{
    [Header("파라미터")]
    [SerializeField] string _IdleBoolName = "IsIdle";
    [SerializeField] string _WalkBoolName = "IsWalk";
    [SerializeField] string _InteractTriggerName = "InteractTrigger";

    Animator _Anim;
    INPCAnimSource _AnimSource;

    void Awake()
    {
        _Anim = GetComponent<Animator>();
        _AnimSource = GetComponent<INPCAnimSource>();

        if (_AnimSource != null)
        {
            _AnimSource.OnAnimStateChanged += OnAnimStateChanged;
            OnAnimStateChanged(_AnimSource.CurrentAnimState);
        }
    }

    void OnDestroy()
    {
        if (_AnimSource != null)
            _AnimSource.OnAnimStateChanged -= OnAnimStateChanged;
    }

    void ResetBools()
    {
        if (_Anim == null) return;

        if (!string.IsNullOrEmpty(_IdleBoolName))
            _Anim.SetBool(_IdleBoolName, false);
        if (!string.IsNullOrEmpty(_WalkBoolName))
            _Anim.SetBool(_WalkBoolName, false);
    }

    void OnAnimStateChanged(NPCAnimState state)
    {
        if (_Anim == null) return;

        ResetBools();

        switch (state)
        {
            case NPCAnimState.Idle:
                if (!string.IsNullOrEmpty(_IdleBoolName))
                    _Anim.SetBool(_IdleBoolName, true);
                break;

            case NPCAnimState.Walk:
                if (!string.IsNullOrEmpty(_WalkBoolName))
                    _Anim.SetBool(_WalkBoolName, true);
                break;

            case NPCAnimState.None:
            default:
                // 아무 상태도 켜지 않음 → Animator 기본 상태에 맡김
                break;
        }
    }

    public void PlayInteract()
    {
        if (_Anim == null) return;
        if (string.IsNullOrEmpty(_InteractTriggerName)) return;

        _Anim.SetTrigger(_InteractTriggerName);
    }
}
