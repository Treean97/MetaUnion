using UnityEngine;

// 직렬화할 데이터 셋
[System.Serializable]
public struct GraphicsSettingsDTO
{
    public int width, height;
    public int targetFps;              // -1(무제한), 30, 60, 120 ...
    public FullScreenMode mode;        // FullScreenWindow / Windowed 등
}
public class GraphicManager : MonoBehaviour, ISaveSection
{
    public static GraphicManager _Inst { get; private set; }

    public string Key => "graphic";

    int _w, _h, _targetFps;
    FullScreenMode _mode;
    bool _vSync;

    public int CurrentWidth  => _w;
    public int CurrentHeight => _h;
    public int CurrentTargetFps => _targetFps;
    public bool CurrentVSync => _vSync;
    public FullScreenMode CurrentMode => _mode;


    void Awake()
    {
        if (_Inst != null) { Destroy(gameObject); return; }
        _Inst = this;
        DontDestroyOnLoad(gameObject);

        _w = Screen.width;
        _h = Screen.height;
        _mode = Screen.fullScreenMode;
        _targetFps = Application.targetFrameRate;    // 보통 -1
        _vSync = QualitySettings.vSyncCount > 0;     // 현재 VSync 상태

        SaveLoadManager._Inst?.Register(this);
        // 초기 적용(저장 파일 없을 때도 Mixer처럼 즉시 반영)
        ApplyDisplay(); 
        ApplyFrameCap();
    }

    // static RefreshRate ToRR(int frameRate)
    // {
    //     RefreshRate rr = new RefreshRate { numerator = (uint)frameRate, denominator = 1 };
    //     return rr;
    // }
    
   public void SetResolution(int w, int h, int hz, bool requestSave = true)
    {
        _w = w; _h = h;
        ApplyDisplay();
        if (requestSave) SaveLoadManager._Inst?.RequestSaveSection(Key);
    }

    public void SetFullscreen(bool on, bool requestSave = true)
    {
        _mode = on ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        ApplyDisplay();
        if (requestSave) SaveLoadManager._Inst?.RequestSaveSection(Key);
    }

    public void SetTargetFps(int fps, bool requestSave = true)
    {
        _targetFps = fps;            // -1/30/60/120...
        ApplyFrameCap();
        if (requestSave) SaveLoadManager._Inst?.RequestSaveSection(Key);
    }

    public void SetVSync(bool on, bool requestSave = true)
    {
        _vSync = on;
        ApplyFrameCap();             // VSync와 FPS 캡 상호작용 정리
        if (requestSave) SaveLoadManager._Inst?.RequestSaveSection(Key);
    }

    void ApplyDisplay()
    {
        Screen.SetResolution(_w, _h, _mode); // RR 인자 생략
    }

    // VSync / FPS 캡 적용
    void ApplyFrameCap()
    {
        if (_vSync)
        {
            // VSync ON: 디스플레이 주사율에 동기화. targetFrameRate는 보통 -1로 둠.
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = -1;
        }
        else
        {
            // VSync OFF: FPS 캡을 직접 적용.
            QualitySettings.vSyncCount = 0;

            Application.targetFrameRate = _targetFps; // -1이면 무제한
        }
    }

    
    // --- 저장/로드 ---
    public string CaptureJson()
    {
        var dto = new GraphicsSettingsDTO
        {
            width = _w,
            height = _h,
            targetFps = _targetFps,
            mode = _mode
        };
        return JsonUtility.ToJson(dto);
    }

    public void ApplyJson(string s)
    {
        var dto = JsonUtility.FromJson<GraphicsSettingsDTO>(s);        

        _w = dto.width; _h = dto.height;
        _mode = dto.mode; _targetFps = dto.targetFps;

        Screen.fullScreenMode = _mode;
        Screen.SetResolution(_w, _h, _mode);

        QualitySettings.vSyncCount = 0;          // FPS 우선
        Application.targetFrameRate = _targetFps;
    }
}
