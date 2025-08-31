using UnityEngine;

public static class CursorManager
{
    // UI에 의해 표시되어야 하는 개수
    private static int  _ShowCount = 0;
    // 수동 토글 상태(Alt 등)
    private static bool _Manual    = false;
    // 마지막 적용된 상태(이벤트/OS 반영 중복 방지)
    private static bool _LastShown = false;

    // 현재 표시 여부
    public static bool _IsShown => (_ShowCount > 0) || _Manual;

    // --- UI가 열릴 때/닫힐 때 ---
    public static void ShowCursor()
    {
        _ShowCount++;
        Apply();
    }

    public static void HideCursor()
    {
        if (_ShowCount <= 0) return;
        _ShowCount--;
        Apply();
    }

    public static void Toggle()
    {
        // UI 활성 중엔 수동 토글 무시
        if (_ShowCount != 0) return; 
        _Manual = !_Manual;
        Apply();
    }

    private static void Apply()
    {
        bool show = _IsShown;
        // UI 활성 중엔 수동 토글 무시
        if (_LastShown == show) return;
        _LastShown = show;

        Cursor.visible   = show;
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
