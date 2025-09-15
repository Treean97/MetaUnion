using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    [SerializeField] Button _CloseButton;

    [Header("Sound Setting")]
    [SerializeField] GameObject _SoundTab;
    [SerializeField] Slider _MasterVolSlider;
    [SerializeField] Slider _BGMVolSlider;
    [SerializeField] Slider _SFXVolSlider;

    [Header("Graphic Setting")]
    [SerializeField] GameObject _GraphicTab;
    [SerializeField] TMP_Dropdown _ResolutionDD;
    [SerializeField] TMP_Dropdown _FrameRateDD;
    [SerializeField] Toggle _VSync;
    [SerializeField] Toggle _FullScreen;

    [Header("Input Setting")]
    [SerializeField] GameObject _InputTab;
    [SerializeField] Slider _SensSlider;
    [SerializeField] Toggle _InvertYToggle;


    private GameObject _DefaultTab;

    // 드롭다운 프리셋
    static readonly Vector2Int[] RES_PRESET =
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1600, 900),
        new Vector2Int(1920,1080),
        new Vector2Int(2560,1440),
        new Vector2Int(3840,2160),
    };

    static readonly int[] FPS_PRESET = { -1, 30, 60, 120 };

    void Awake()
    {
        _CloseButton.onClick.AddListener(OnClickCloseButton);

        _DefaultTab = _SoundTab;
        DefaultSet();
    }

    void OnEnable()
    {
        // 바인딩
        BindAudio();
        BindGraphics();
        BindInput();
    }

    void OnDisable()
    {
        // Audio
        _MasterVolSlider.onValueChanged.RemoveAllListeners();
        _BGMVolSlider.onValueChanged.RemoveAllListeners();
        _SFXVolSlider.onValueChanged.RemoveAllListeners();

        // Graphic
        _ResolutionDD.onValueChanged.RemoveAllListeners();
        _FrameRateDD.onValueChanged.RemoveAllListeners();
        _VSync.onValueChanged.RemoveAllListeners();
        _FullScreen.onValueChanged.RemoveAllListeners();

        // Input
        _SensSlider.onValueChanged.RemoveAllListeners();
        _InvertYToggle.onValueChanged.RemoveAllListeners();
    }

    void DefaultSet()
    {
        _SoundTab.SetActive(false);
        _GraphicTab.SetActive(false);
        _InputTab.SetActive(false);

        _DefaultTab.SetActive(true);
    }

    void OnClickCloseButton()
    {
        SaveLoadManager._Inst.SaveAll();
        gameObject.SetActive(false);
    }

    void BindAudio()
    {
        var audio = AudioManager._Inst;
        if (!audio) return;

        _MasterVolSlider.SetValueWithoutNotify(audio.GetMasterValue());
        _BGMVolSlider.SetValueWithoutNotify(audio.GetBGMValue());
        _SFXVolSlider.SetValueWithoutNotify(audio.GetSFXValue());

        _MasterVolSlider.onValueChanged.AddListener(v => audio.SetMasterValue(v));
        _BGMVolSlider.onValueChanged.AddListener(v => audio.SetBGMValue(v));
        _SFXVolSlider.onValueChanged.AddListener(v => audio.SetSFXValue(v));
    }

    void BindGraphics()
    {
        var graphic = GraphicManager._Inst;
        if (!graphic) return;

        // 해상도 드롭다운
        var resOptions = new List<TMP_Dropdown.OptionData>(RES_PRESET.Length);
        int selectedResIndex = 0;
        for (int i = 0; i < RES_PRESET.Length; i++)
        {
            var r = RES_PRESET[i];
            resOptions.Add(new TMP_Dropdown.OptionData($"{r.x} × {r.y}"));
            if (r.x == graphic.CurrentWidth && r.y == graphic.CurrentHeight) selectedResIndex = i;
        }
        _ResolutionDD.ClearOptions();
        _ResolutionDD.AddOptions(resOptions);
        _ResolutionDD.SetValueWithoutNotify(selectedResIndex);
        _ResolutionDD.onValueChanged.AddListener(i =>
        {
            var r = RES_PRESET[i];
            GraphicManager._Inst?.SetResolution(r.x, r.y);
        });

        // FPS 드롭다운
        var fpsOptions = new List<TMP_Dropdown.OptionData>(FPS_PRESET.Length);
        int selectedFpsIndex = 0;
        for (int i = 0; i < FPS_PRESET.Length; i++)
        {
            int f = FPS_PRESET[i];
            fpsOptions.Add(new TMP_Dropdown.OptionData(f < 0 ? "제한 없음" : $"{f} FPS"));
            if (f == graphic.CurrentTargetFps) selectedFpsIndex = i;
        }
        _FrameRateDD.ClearOptions();
        _FrameRateDD.AddOptions(fpsOptions);
        _FrameRateDD.SetValueWithoutNotify(selectedFpsIndex);
        _FrameRateDD.interactable = !graphic.CurrentVSync; // VSync면 비활성화
        _FrameRateDD.onValueChanged.AddListener(i =>
        {
            GraphicManager._Inst?.SetTargetFps(FPS_PRESET[i]);
        });

        // VSync 토글
        _VSync.SetIsOnWithoutNotify(graphic.CurrentVSync);
        _VSync.onValueChanged.AddListener(on =>
        {
            GraphicManager._Inst?.SetVSync(on);
            _FrameRateDD.interactable = !on; // VSync ON이면 FPS 변경 비활성
        });

        // 전체화면 토글(보더리스=ON, 창모드=OFF)
        bool isFullscreen = graphic.CurrentMode != FullScreenMode.Windowed;
        _FullScreen.SetIsOnWithoutNotify(isFullscreen);
        _FullScreen.onValueChanged.AddListener(on =>
        {
            GraphicManager._Inst?.SetFullscreen(on);
        });
    }
    
    void BindInput()
    {
        var input = InputManager._Inst;
        if (!input) return;

        _SensSlider.SetValueWithoutNotify(input.GetLookSensitivity());
        _InvertYToggle.SetIsOnWithoutNotify(input.IsInvertY());

        _SensSlider.onValueChanged.AddListener(v => InputManager._Inst?.SetLookSensitivity(v));
        _InvertYToggle.onValueChanged.AddListener(on => InputManager._Inst?.SetInvertY(on));
    } 
    
}
