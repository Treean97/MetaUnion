using UnityEngine;

public class CursorGuard : MonoBehaviour
{
    bool _Armed;

    void OnEnable()
    {
        if (_Armed) return;
        CursorManager.ShowCursor();
        _Armed = true;
    }

    void OnDisable()
    {
        if (!_Armed) return;
        CursorManager.HideCursor();
        _Armed = false;
    }

    void OnDestroy() => OnDisable(); // 누수 방지
}
