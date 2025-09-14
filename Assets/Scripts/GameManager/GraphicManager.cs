using UnityEngine;

// 직렬화할 데이터 셋
[System.Serializable]
public struct GraphicsSettingsDTO
{
    public int Width, Height;
    public int TargetFps;              // -1(무제한), 30, 60, 120 ...
    public bool VSync;
    public FullScreenMode Mode;        // FullScreenWindow / Windowed 등
}
public class GraphicManager : MonoBehaviour, ISaveSection
{
    public static GraphicManager _Inst { get; private set; }
    public string Key => "graphic";

    [SerializeField] int _DefaultWidth = 1920;
    [SerializeField] int _DefaultHeight = 1080;
    [SerializeField] int _DefaultTargetFPS = -1;

    int _W, _H, _TargetFps;
    FullScreenMode _Mode;
    bool _VSync;

    public int CurrentWidth  => _W;
    public int CurrentHeight => _H;
    public int CurrentTargetFps => _TargetFps;
    public bool CurrentVSync => _VSync;
    public FullScreenMode CurrentMode => _Mode;


    void Awake()
    {
        if (_Inst != null) { Destroy(gameObject); return; }
        _Inst = this;
        DontDestroyOnLoad(gameObject);

        DefaultSet();

        SaveLoadManager._Inst?.Register(this);
        // 초기 적용(저장 파일 없을 때도 Mixer처럼 즉시 반영)
        ApplyDisplay(); 
        ApplyFrameCap();
    }

    void DefaultSet()
    {
        _W = _DefaultWidth;
        _H = _DefaultHeight;
        _Mode = FullScreenMode.Windowed;
        _TargetFps = _DefaultTargetFPS;
        _VSync = false;
    }

    // static RefreshRate ToRR(int frameRate)
    // {
    //     RefreshRate rr = new RefreshRate { numerator = (uint)frameRate, denominator = 1 };
    //     return rr;
    // }

    public void SetResolution(int w, int h, bool requestSave = true)
    {
        _W = w; _H = h;
        ApplyDisplay();
        if (requestSave) SaveLoadManager._Inst?.RequestSaveSection(Key);
    }

    public void SetFullscreen(bool on, bool requestSave = true)
    {
        _Mode = on ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        ApplyDisplay();
        if (requestSave) SaveLoadManager._Inst?.RequestSaveSection(Key);
    }

    public void SetTargetFps(int fps, bool requestSave = true)
    {
        _TargetFps = fps;            // -1/30/60/120...
        ApplyFrameCap();
        if (requestSave) SaveLoadManager._Inst?.RequestSaveSection(Key);
    }

    public void SetVSync(bool on, bool requestSave = true)
    {
        _VSync = on;
        ApplyFrameCap();             // VSync와 FPS 캡 상호작용 정리
        if (requestSave) SaveLoadManager._Inst?.RequestSaveSection(Key);
    }

    void ApplyDisplay()
    {
        Screen.SetResolution(_W, _H, _Mode); // RR 인자 생략
    }

    // VSync / FPS 캡 적용
    void ApplyFrameCap()
    {
        if (_VSync)
        {
            // VSync ON: 디스플레이 주사율에 동기화. targetFrameRate는 보통 -1로 둠.
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = -1;
        }
        else
        {
            // VSync OFF: FPS 캡을 직접 적용.
            QualitySettings.vSyncCount = 0;

            Application.targetFrameRate = _TargetFps; // -1이면 무제한
        }
    }

    
    // --- 저장/로드 ---
    public string CaptureJson()
    {
        var dto = new GraphicsSettingsDTO
        {
            Width = _W,
            Height = _H,
            TargetFps = _TargetFps,
            VSync = _VSync,
            Mode = _Mode
        };
        return JsonUtility.ToJson(dto);
    }

    public void ApplyJson(string s)
    {
        var dto = JsonUtility.FromJson<GraphicsSettingsDTO>(s);

        _W = dto.Width; _H= dto.Height;
        _Mode = dto.Mode;
        _TargetFps = dto.TargetFps;
        _VSync = dto.VSync; // 추가

        ApplyDisplay();
        ApplyFrameCap();
    }
}
