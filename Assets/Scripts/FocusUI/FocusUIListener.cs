using UnityEngine;

public class FocusUIListener : MonoBehaviour
{
    [SerializeField] FocusUIManager _FocusUIPanel;

    InfoDataSO _LastInfo;   // 마지막으로 포커스된 정보 캐시
    bool _IsShowing;

    void OnEnable()
    {
        GameEvents.OnFocus += HandleFocus;
        GameEvents.OnDefocus += HandleDefocus;

        FocusUIBlockManager.OnFocusUIBlockStatus += HandleFocusUIBlockStatus;
    }

    void OnDisable()
    {
        GameEvents.OnFocus -= HandleFocus;
        GameEvents.OnDefocus -= HandleDefocus;

        FocusUIBlockManager.OnFocusUIBlockStatus -= HandleFocusUIBlockStatus;
    }

    private void HandleFocus(InfoDataSO objInfo)
    {
        _LastInfo = objInfo;

        if (FocusUIBlockManager.IsFocusUIBlocked)
            return; // 블록 중엔 "표시만" 하지 말고 캐시만 갱신

        ShowWith(_LastInfo);
    }

    private void HandleDefocus()
    {
        _LastInfo = null;
        Hide();
    }

    private void HandleFocusUIBlockStatus(bool isBlocked)
    {
        if (isBlocked)
        {
            Hide(); // 블록되면 무조건 숨김
            return;
        }

        // 언블록되면, 마지막 포커스 정보가 있으면 즉시 복구 표시
        if (_LastInfo != null)
            ShowWith(_LastInfo);
    }

    void ShowWith(InfoDataSO info)
    {
        if (info == null) return;

        _FocusUIPanel.Setup(info);

        if (!_IsShowing)
        {
            UIFX.Show(_FocusUIPanel.gameObject);
            _IsShowing = true;
        }
    }

    void Hide()
    {
        if (!_IsShowing) return;

        UIFX.Hide(_FocusUIPanel.gameObject);
        _IsShowing = false;
    }
}
