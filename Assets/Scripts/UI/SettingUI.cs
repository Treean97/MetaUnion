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

    private GameObject _DefaultTab;

    void Awake()
    {
        _CloseButton.onClick.AddListener(OnClickCloseButton);

        _DefaultTab = _SoundTab;
        DefaultSet();
    }

    void OnEnable()
    {
        // 초기 값 UI 반영
        var a = AudioManager._Inst;
        if (a)
        {
            _MasterVolSlider.SetValueWithoutNotify(a.GetMasterValue());
            _BGMVolSlider.SetValueWithoutNotify(a.GetBGMValue());
            _SFXVolSlider.SetValueWithoutNotify(a.GetSFXValue());

            // 변경 → 매니저 세터 호출(세터가 저장 요청까지 수행)
            _MasterVolSlider.onValueChanged.AddListener(v => a.SetMasterValue(v));
            _BGMVolSlider.onValueChanged.AddListener(v => a.SetBGMValue(v));
            _SFXVolSlider.onValueChanged.AddListener(v => a.SetSFXValue(v));
        }
    }
    
    void OnDisable()
    {
        _MasterVolSlider.onValueChanged.RemoveAllListeners();
        _BGMVolSlider.onValueChanged.RemoveAllListeners();
        _SFXVolSlider.onValueChanged.RemoveAllListeners();
    }

    void DefaultSet()
    {
        _SoundTab.SetActive(false);
        _GraphicTab.SetActive(false);


        _DefaultTab.SetActive(true);
    }

    void OnClickCloseButton()
    {
        SaveLoadManager._Inst.SaveAll();
        gameObject.SetActive(false);
    }
    
}
