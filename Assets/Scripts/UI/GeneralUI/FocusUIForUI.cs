using TMPro;
using UnityEngine;

public class FocusUIForUI : MonoBehaviour
{
    [SerializeField] private RectTransform _FocusUI;
    [SerializeField] private TMP_Text _Name;
    [SerializeField] private TMP_Text _Desc;

    private bool _IsHovering;
    private InfoDataSO _LastInfo;

    void OnEnable()
    {
        FocusableUI.OnPointerEnterFocusUI += HandleOnPointerEnterFocusUI;
        FocusableUI.OnPointerExitFocusUI += HandleOnPointerExitFocusUI;

        FocusUIBlockManager.OnFocusUIBlockStatus += HandleFocusUIBlockStatus;
    }

    void OnDisable()
    {
        FocusableUI.OnPointerEnterFocusUI -= HandleOnPointerEnterFocusUI;
        FocusableUI.OnPointerExitFocusUI -= HandleOnPointerExitFocusUI;

        FocusUIBlockManager.OnFocusUIBlockStatus -= HandleFocusUIBlockStatus;
    }

    void HandleOnPointerEnterFocusUI(InfoDataSO itemInfo)
    {
        if (itemInfo == null) return;

        _IsHovering = true;
        _LastInfo = itemInfo;

        // 블록 중이면 캐시만 갱신하고 표시하지 않음
        if (FocusUIBlockManager.IsFocusUIBlocked)
            return;

        Show(_LastInfo);
    }

    void HandleOnPointerExitFocusUI()
    {
        _IsHovering = false;
        _LastInfo = null;

        HideAndClear();
    }

    void HandleFocusUIBlockStatus(bool isBlocked)
    {
        if (isBlocked)
        {
            // 블록되면 숨기되 캐시는 유지(언블록 시 복구를 위해)
            _FocusUI.gameObject.SetActive(false);
            return;
        }

        // 언블록되면, 현재 hover 상태라면 다시 표시
        if (_IsHovering && _LastInfo != null)
            Show(_LastInfo);
    }

    void Show(InfoDataSO info)
    {
        if (info == null) return;

        _Name.text = info.DisplayName;
        _Desc.text = info.Description;
        _FocusUI.gameObject.SetActive(true);
    }

    void HideAndClear()
    {
        _FocusUI.gameObject.SetActive(false);
        _Name.text = "";
        _Desc.text = "";
    }

    void Update()
    {
        if (!_FocusUI.gameObject.activeSelf)
            return;

        Vector2 mousePos = Input.mousePosition;

        float w = _FocusUI.rect.width;
        float h = _FocusUI.rect.height;

        float x = Mathf.Clamp(mousePos.x, 0, Screen.width - w);
        float y = Mathf.Clamp(mousePos.y, 0, Screen.height - h);

        _FocusUI.transform.position = new Vector3(x, y, 0);
    }
}
