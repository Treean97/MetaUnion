using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISound : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] bool _ClickSound;
    [SerializeField] bool _HoverSound;
    [SerializeField] bool _OpenSound;
    [SerializeField] bool _CloseSound;
    [SerializeField] bool _IsToggle;

    private Button _Button;
    private Toggle _Toggle;

    void PlayClick() => AudioManager._Inst.PlayLocalByKey("UIClick");
    void PlayHover() => AudioManager._Inst.PlayLocalByKey("UIHover");
    void PlayOpen() => AudioManager._Inst.PlayLocalByKey("UIOpen");
    void PlayClose() => AudioManager._Inst.PlayLocalByKey("UIClose");
    void PlayToggleOn() => AudioManager._Inst.PlayLocalByKey("ToggleOn");
    void PlayToggleOff() => AudioManager._Inst.PlayLocalByKey("ToggleOff");

    void Awake()
    {
        _Button = GetComponent<Button>(); // 버튼이 없을 수도 있음(패널, 이미지 등)
        _Toggle = GetComponent<Toggle>(); //
        if (_Button) _Button.onClick.AddListener(OnClick);    
        if (_Toggle) _Toggle.onValueChanged.AddListener(OnToggled);
    }

    void OnDestroy()
    {
        if (_Button) _Button.onClick.RemoveListener(OnClick);
        if (_Toggle) _Toggle.onValueChanged.RemoveListener(OnToggled);
    }

    void OnEnable()
    {
        if (!_OpenSound) return;

        PlayOpen();

    }

    void OnDisable()
    {
        if (!_CloseSound) return;

        PlayClose();
    }

    

    public void OnClick()
    {
        if (!_ClickSound) return;
        if (!_Button || !_Button.interactable) return;
        PlayClick();
    }

    void OnToggled(bool isOn)
    {
        if (!_IsToggle) return;
        // 비활성/막힌 버튼이면 무음
        if (!_Toggle.interactable) return;

        var es = EventSystem.current;
        if (es == null) return;
        if (es.currentSelectedGameObject != gameObject) return; // 사용자가 누른 토글이 아님(프로그램/그룹에 의해 변경)

        // 상태별 키 재생 (2D UI)
        if (isOn) PlayToggleOn();
        else PlayToggleOff();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_HoverSound) return;
        if (!_Button || !_Button.interactable) return;
        PlayHover();
    }


}
