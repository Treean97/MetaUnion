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

    private GameObject _DefaultTab;

    void Awake()
    {
        _CloseButton.onClick.AddListener(OnClickCloseButton);

        _DefaultTab = _SoundTab;
        DefaultSet();
    }

    void DefaultSet()
    {
        _SoundTab.SetActive(false);
        _GraphicTab.SetActive(false);


        _DefaultTab.SetActive(true);
    }

    void OnClickCloseButton()
    {
        gameObject.SetActive(false);
    }
    
}
