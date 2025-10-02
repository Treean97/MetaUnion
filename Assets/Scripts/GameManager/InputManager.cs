using System;
using UnityEngine;

[Serializable]
public struct InputSettingsDTO
{
    public float Sens; // 마우스 시점 감도(0~1 or 임의범위)
    public float ZoomSpeed; // 줌 속도
    public bool InvertY; // Y축 반전    
}

public class InputManager : MonoBehaviour, ILocalSaveSection
{
    public static InputManager _Inst { get; private set; }

    public string Key => "input";


    [Header("Default Set")]
    [Range(0f, 1f)]
    [SerializeField] private float _DefaultSens = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float _DefaultZoomSpeed = 0.5f;
    [SerializeField] private bool _DefaultInvertY = false;

    float _Sens;
    float _ZoomSpeed;
    bool  _InvertY;

    public float GetSensitivity() => _Sens;
    public float GetZoomSpeed() => _ZoomSpeed;
    public bool IsInvertY() => _InvertY;

    void Awake()
    {
        if (_Inst != null && _Inst != this)
        {
            Destroy(this);
            return;
        }
        _Inst = this;
        DontDestroyOnLoad(gameObject);

        DefaultSet();
    }

    void Start()
    {
        SaveLoadManager._Inst?.RegisterLocal(this);
    }

    void DefaultSet()
    {
        _Sens = _DefaultSens;
        _ZoomSpeed = _DefaultZoomSpeed;
        _InvertY = _DefaultInvertY;
    }

    public void SetSensitivity(float v, bool requestSave = true)
    {
        var clamped = Mathf.Clamp01(v);
        if (Mathf.Approximately(_Sens, clamped)) return; // 불필요 저장 방지
        _Sens = clamped;
        if (requestSave) SaveLoadManager._Inst?.SaveLocalSection(Key);
        
    }

    public void SetInvertY(bool on, bool requestSave = true)
    {
        if (_InvertY == on) return;
        _InvertY = on;
        if (requestSave) SaveLoadManager._Inst?.SaveLocalSection(Key);        
    }

    public void SetZoomSpeed(float v, bool requestSave = true)
    {
        var c = Mathf.Clamp01(v);
        if (Mathf.Approximately(_ZoomSpeed, c)) return;
        _ZoomSpeed = c;
        if (requestSave) SaveLoadManager._Inst?.SaveLocalSection(Key);
    }


    public void ApplyJson(string s)
    {
        if (string.IsNullOrEmpty(s)) return;
        InputSettingsDTO dto;
        try { dto = JsonUtility.FromJson<InputSettingsDTO>(s); }
        catch { return; }

        // 저장 반영 시에는 requestSave=false (재저장 루프 방지)
        SetSensitivity(Mathf.Clamp01(dto.Sens), false);
        SetZoomSpeed(Mathf.Clamp01(dto.ZoomSpeed), false);
        SetInvertY(dto.InvertY, false);
    }

    public string CaptureJson()
    {
        var dto = new InputSettingsDTO
        {
            Sens = _Sens,
            InvertY = _InvertY,
            ZoomSpeed = _ZoomSpeed
        };
        
        return JsonUtility.ToJson(dto);
    }

}
