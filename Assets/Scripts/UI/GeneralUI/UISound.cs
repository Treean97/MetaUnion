using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISound : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] SoundSO uiSet; // Entries: "UIClick", "UIHover", "UIPop", "UIClose"
    [SerializeField] bool _ClickSound;
    [SerializeField] bool _HoverSound;
    [SerializeField] bool _OpenSound;
    [SerializeField] bool _CloseSound;

    private Button _Button;

    void PlayClick() => AudioManager._Inst.PlayLocalFromSO(uiSet, "UIClick");
    void PlayHover() => AudioManager._Inst.PlayLocalFromSO(uiSet, "UIHover");
    void PlayOpen() => AudioManager._Inst.PlayLocalFromSO(uiSet, "UIPop");
    void PlayClose() => AudioManager._Inst.PlayLocalFromSO(uiSet, "UIClose");

    void Awake()
    {
        TryGetComponent(out _Button);                 // 버튼이 없을 수도 있음(패널, 이미지 등)
        if (_Button) _Button.onClick.AddListener(OnClick);               
    }

    void OnDestroy()
    {
        if (_Button) _Button.onClick.RemoveListener(OnClick);
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

        var button = gameObject.GetComponent<Button>();
        if (!button.interactable) return;
        PlayClick();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_HoverSound) return;

        var button = gameObject.GetComponent<Button>();
        if (!button || !button.interactable) return;
        PlayHover();
    }


}
