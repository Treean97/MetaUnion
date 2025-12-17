using UnityEngine;

public class FocusUIBlockGuard : MonoBehaviour
{
    bool _Armed;

    void OnEnable()
    {
        if (_Armed) return;
        FocusUIBlockManager.BlockFocusUI();
        _Armed = true;
    }

    void OnDisable()
    {
        if (!_Armed) return;
        FocusUIBlockManager.UnblockFocusUI();
        _Armed = false;
    }

    void OnDestroy() => OnDisable(); // 누수 방지
}
