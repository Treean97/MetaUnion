using UnityEngine;

[RequireComponent(typeof(MountEntity))]
public class VehicleSound : MonoBehaviour
{
    [Header("Sound Keys")]
    [SerializeField] private string _CarIdleKey  = "CarIdle";
    [SerializeField] private string _CarAccelKey = "CarAccel";
    [SerializeField] private string _CarDecelKey = "CarDecel";

    [Header("Strength (%) - Script Only")]
    [Range(0f, 100f)] [SerializeField] private float _IdlePct  = 35f;
    [Range(0f, 100f)] [SerializeField] private float _AccelPct = 65f;
    [Range(0f, 100f)] [SerializeField] private float _DecelPct = 55f;

    [Header("Accel/Decel Detect (km/h per sec)")]
    [SerializeField] private float _AccelDeadzone = 1.0f;
    [SerializeField] private float _AccelFull     = 12.0f;
    [SerializeField] private float _DecelDeadzone = 1.0f;
    [SerializeField] private float _DecelFull     = 12.0f;

    [Header("Fade")]
    [SerializeField] private float _FadeSpeed = 8f;

    Pooled3DAudioPlayer _IdleP, _AccelP, _DecelP;
    float _IdleBaseVol, _AccelBaseVol, _DecelBaseVol;

    bool _Running;

    Vector3 _LastPos;
    float _LastSpeedKmh;

    float _IdleVolCur, _AccelVolCur, _DecelVolCur;

    void Awake()
    {
        _LastPos = transform.position;
    }

    void OnDisable()
    {
        StopEngine();
    }

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

        _LastPos = transform.position;
        _LastSpeedKmh = 0f;
        _IdleVolCur = _AccelVolCur = _DecelVolCur = 0f;

        if (!string.IsNullOrEmpty(_CarIdleKey))
            _IdleP = AudioManager._Inst.Play3DAttachedLoopByKey(_CarIdleKey, transform, 0f, out _IdleBaseVol);

        if (!string.IsNullOrEmpty(_CarAccelKey))
            _AccelP = AudioManager._Inst.Play3DAttachedLoopByKey(_CarAccelKey, transform, 0f, out _AccelBaseVol);

        if (!string.IsNullOrEmpty(_CarDecelKey))
            _DecelP = AudioManager._Inst.Play3DAttachedLoopByKey(_CarDecelKey, transform, 0f, out _DecelBaseVol);
    }

    void StopEngine()
    {
        if (_IdleP  != null) { _IdleP.StopAndReturn();  _IdleP = null; }
        if (_AccelP != null) { _AccelP.StopAndReturn(); _AccelP = null; }
        if (_DecelP != null) { _DecelP.StopAndReturn(); _DecelP = null; }

        _IdleBaseVol = _AccelBaseVol = _DecelBaseVol = 0f;
    }

    void Update()
    {
        if (!_Running) return;

        float dt = Time.deltaTime;
        if (dt <= 0.0001f) return;

        Vector3 pos = transform.position;
        float speedKmh = ((pos - _LastPos) / dt).magnitude * 3.6f;
        float accelKmhPerSec = (speedKmh - _LastSpeedKmh) / dt;

        _LastPos = pos;
        _LastSpeedKmh = speedKmh;

        float accelW = 0f;
        if (accelKmhPerSec > _AccelDeadzone)
            accelW = Mathf.Clamp01(accelKmhPerSec / Mathf.Max(0.01f, _AccelFull));

        float decelW = 0f;
        if (accelKmhPerSec < -_DecelDeadzone)
            decelW = Mathf.Clamp01((-accelKmhPerSec) / Mathf.Max(0.01f, _DecelFull));

        float idleW = 1f - Mathf.Clamp01(Mathf.Max(accelW, decelW));

        float idleMul  = Mathf.Clamp01(_IdlePct  / 100f);
        float accelMul = Mathf.Clamp01(_AccelPct / 100f);
        float decelMul = Mathf.Clamp01(_DecelPct / 100f);

        float idleTarget  = _IdleBaseVol  * idleMul  * idleW;
        float accelTarget = _AccelBaseVol * accelMul * accelW;
        float decelTarget = _DecelBaseVol * decelMul * decelW;

        float k = Mathf.Clamp01(_FadeSpeed * dt);
        _IdleVolCur  = Mathf.Lerp(_IdleVolCur,  idleTarget,  k);
        _AccelVolCur = Mathf.Lerp(_AccelVolCur, accelTarget, k);
        _DecelVolCur = Mathf.Lerp(_DecelVolCur, decelTarget, k);

        if (_IdleP  != null) _IdleP.SetVolume(_IdleVolCur);
        if (_AccelP != null) _AccelP.SetVolume(_AccelVolCur);
        if (_DecelP != null) _DecelP.SetVolume(_DecelVolCur);
    }
}
