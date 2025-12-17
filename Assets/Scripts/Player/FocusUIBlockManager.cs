using System;
using UnityEngine;

public static class FocusUIBlockManager
{
    // Focus UI를 숨기라고 요청한 객체 개수
    private static int _BlockCount = 0;

    // true: 숨김, false: 표시
    public static event Action<bool> OnFocusUIBlockStatus;

    public static void BlockFocusUI()
    {
        Debug.Log("BlockFocusUI");

        if (_BlockCount == 0)
            OnFocusUIBlockStatus?.Invoke(true);

        _BlockCount++;
    }

    public static void UnblockFocusUI()
    {
        Debug.Log("UnblockFocusUI");

        if (_BlockCount <= 0) return;

        _BlockCount--;

        if (_BlockCount == 0)
            OnFocusUIBlockStatus?.Invoke(false);
    }

    public static bool IsFocusUIBlocked => _BlockCount > 0;
}
