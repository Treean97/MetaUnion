using UnityEngine;

public class CursorGuard : MonoBehaviour
{
    bool _Armed;

    void OnEnable()
    {
        if (_Armed) return;
        CursorManager.PushUI();
        _Armed = true;
    }

    void OnDisable()
    {
        if (!_Armed) return;
        CursorManager.PopUI();
        _Armed = false;
    }

    void OnDestroy() => OnDisable(); // 누수 방지
}
