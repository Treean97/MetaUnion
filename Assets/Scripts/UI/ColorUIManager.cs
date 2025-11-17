using UnityEngine;
using UnityEngine.UI;

public class ColorUIManager : MonoBehaviour, IColorUI
{
    [Header("색상표")]
    [SerializeField] FlexibleColorPicker _ColorPicker;

    [Header("버튼")]
    [SerializeField] Button _ApplyButton;

    CustomizeItemSO _CurrentItem;

    public bool IsOpen => gameObject.activeSelf;

    void Awake()
    {
        if (_ColorPicker != null)
            _ColorPicker.onColorChange.AddListener(OnColorChanged);

        if (_ApplyButton != null)
            _ApplyButton.onClick.AddListener(OnClickApply);
    }

    void OnDestroy()
    {
        if (_ColorPicker != null)
            _ColorPicker.onColorChange.RemoveListener(OnColorChanged);

        if (_ApplyButton != null)
            _ApplyButton.onClick.RemoveListener(OnClickApply);
    }

    // IUI 구현 --------------------------------------
    public void Show() { }

    public void Hide()
    {
        _CurrentItem = null;
    }

    // IColorUI 구현 ----------------------------------
    public void SetUI(CustomizeItemSO item)
    {
        _CurrentItem = item;

        // ※ 아직 저장된 색 불러오는 구조는 없으니 기본값
        if (_ColorPicker != null)
            _ColorPicker.color = Color.white;
    }

    // FlexibleColorPicker에서 색이 바뀔 때마다 호출 (프리뷰용)
    void OnColorChanged(Color color)
    {
        if (_CurrentItem == null) return;

        // ▶ 프리뷰 모델만 변경 요청
        GameEvents.RaiseRequestPreviewItemColor(_CurrentItem, color);
    }

    // 적용 버튼을 눌렀을 때: 실제 플레이어에 색 적용 + 저장/전파
    void OnClickApply()
    {
        if (_CurrentItem == null || _ColorPicker == null) return;

        var color = _ColorPicker.color;

        // 실제 플레이어에 적용 + 저장/전파
        GameEvents.RaiseRequestApplyItemColor(_CurrentItem, color);
    }
}
