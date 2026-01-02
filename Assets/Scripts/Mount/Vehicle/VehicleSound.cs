using UnityEngine;

[RequireComponent(typeof(MountEntity))]
public class VehicleSound : MonoBehaviour
{
    [Header("Sound Key (Single Loop)")]
    [SerializeField] private string _AccelKey = "CarAccel"; // 엔진 루프 1개만 사용

    [Header("Strength (%) - Script Only")]
    [Range(0f, 100f)] [SerializeField] private float _IdlePct = 35f; // 정지/저속 구간 볼륨 배율
    [Range(0f, 100f)] [SerializeField] private float _RunPct  = 65f; // 주행 구간 볼륨 배율

    [Header("Idle by Speed Percent")]
    [Range(0f, 0.3f)] [SerializeField] private float _IdleEnterPct = 0.05f; // 5% 이하이면 Idle 강제
    [Range(0f, 0.3f)] [SerializeField] private float _IdleExitPct  = 0.07f; // 7% 이상이면 Idle 해제(깜빡임 방지)

    [Header("Pitch by Speed Percent")]
    [SerializeField] private float _IdlePitch   = 0.90f;
    [SerializeField] private float _RunPitchMin = 0.95f;
    [SerializeField] private float _RunPitchMax = 1.35f;

    [Header("Fade")]
    [SerializeField] private float _FadeSpeed = 8f;

    MountEntity _Mount;
    PrometeoVehicleDataSO _Data;

    Pooled3DAudioPlayer _EngineP;
    float _EngineBaseVol;   // SoundSO Entry의 e.Volume
    bool _Running;

    Vector3 _LastPos;
    float _VolCur;
    float _PitchCur;
    bool _ForceIdle;

    void Awake()
    {
        _Mount = GetComponent<MountEntity>();
        _Data = _Mount ? _Mount.Data as PrometeoVehicleDataSO : null;
        _LastPos = transform.position;
    }

    void OnDisable()
    {
        StopEngine();
    }

    // MountEntity에서 운전석 탑승/하차에 맞춰 호출
    public void SetRunning(bool running)
    {
        if (_Running == running) return;
        _Running = running;

        if (_Running) StartEngine();
        else StopEngine();
    }

    void StartEngine()
    {
        if (AudioManager._Inst == null) return;
        if (string.IsNullOrEmpty(_AccelKey)) return;

        _LastPos = transform.position;
        _VolCur = 0f;
        _PitchCur = _IdlePitch;
        _ForceIdle = true;

        // 키로 3D Attach Loop 렌트 + 재생(볼륨 0에서 시작, Update에서 제어)
        _EngineP = AudioManager._Inst.Play3DAttachedLoopByKey(_AccelKey, transform, 0f, out _EngineBaseVol);
        if (_EngineP != null) _EngineP.SetPitch(_PitchCur);
    }

    void StopEngine()
    {
        if (_EngineP != null) { _EngineP.StopAndReturn(); _EngineP = null; }
        _EngineBaseVol = 0f;
        _VolCur = 0f;
        _PitchCur = _IdlePitch;
        _ForceIdle = false;
    }

    void Update()
    {
        if (!_Running) return;
        if (_EngineP == null) return;

        float dt = Time.deltaTime;
        if (dt <= 0.0001f) return;

        // 원격에서도 동작하도록 Transform 이동량 기반 속도 추정
        Vector3 pos = transform.position;
        float speedKmh = ((pos - _LastPos) / dt).magnitude * 3.6f; // 방향 무시(앞/뒤 구분 없음)
        _LastPos = pos;

        // MaxSpeed 기반 0~1(=0~100%) 정규화
        int maxSpeed = (_Data != null) ? Mathf.Max(1, _Data.MaxSpeed) : 90;
        float speed01 = Mathf.Clamp01(speedKmh / maxSpeed);

        // Idle 강제(히스테리시스)
        if (!_ForceIdle && speed01 <= _IdleEnterPct) _ForceIdle = true;
        else if (_ForceIdle && speed01 >= _IdleExitPct) _ForceIdle = false;

        // % 강도(스크립트 정의) → 0~1
        float idleMul = Mathf.Clamp01(_IdlePct / 100f);
        float runMul  = Mathf.Clamp01(_RunPct  / 100f);

        // 속도% 기반 변화(0~1)
        // - 볼륨: 저속에서도 너무 죽지 않게 바닥을 약간 둠
        // - 피치: speed01로 선형/부드러운 상승
        float speedT = Mathf.SmoothStep(0f, 1f, speed01);

        float volTarget;
        float pitchTarget;

        if (_ForceIdle)
        {
            volTarget = _EngineBaseVol * idleMul;
            pitchTarget = _IdlePitch;
        }
        else
        {
            // 주행 볼륨 = 기본볼륨 * Run% * (0.25 ~ 1.0) * speedT
            float runGain = 0.25f + 0.75f * speedT;
            volTarget = _EngineBaseVol * runMul * runGain;

            pitchTarget = Mathf.Lerp(_RunPitchMin, _RunPitchMax, speedT);
        }

        float k = Mathf.Clamp01(_FadeSpeed * dt);
        _VolCur = Mathf.Lerp(_VolCur, volTarget, k);
        _PitchCur = Mathf.Lerp(_PitchCur, pitchTarget, k);

        _EngineP.SetVolume(_VolCur);
        _EngineP.SetPitch(_PitchCur);
    }
}
