using System;
using UnityEngine;

public static class InputBlockManager
{
    // 열려있는 Input을 막는 객체의 갯수
    private static int _BlockCount = 0;

    public static event Action<bool> OnInputBlockStatus;

    // 팝업 열 때 호출
    public static void BlockInput()
    {
        Debug.Log("BlockInput");
        if (_BlockCount == 0)
        {
            OnInputBlockStatus?.Invoke(true);
        }            

        _BlockCount++;
    }

    // 팝업 닫을 때 호출
    public static void UnblockInput()
    {
        Debug.Log("UnBlockInput");
        if (_BlockCount <= 0) return;

        _BlockCount--;

        if (_BlockCount == 0)
        {
            OnInputBlockStatus?.Invoke(false);
        }
            
    }

    // 외부에서 차단 여부 조회할 때
    public static bool IsInputBlocked => _BlockCount > 0;
}
