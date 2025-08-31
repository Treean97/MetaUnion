using UnityEngine;

public class InputBlockGuard : MonoBehaviour
{
    bool _Armed;

    void OnEnable()
    {
        if (_Armed) return;
        InputBlockManager.BlockInput();
        _Armed = true;
    }

    void OnDisable()
    {
        if (!_Armed) return;
        InputBlockManager.UnblockInput();
        _Armed = false;
    }

    void OnDestroy() => OnDisable(); // 누수 방지
}
