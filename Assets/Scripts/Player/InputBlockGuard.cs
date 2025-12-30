using UnityEngine;

public class InputBlockGuard : MonoBehaviour
{
    [SerializeField] private InputLock _Locks = InputLock.None;
    bool _Armed;

    void OnEnable()
    {
        if (_Armed) return;
        InputBlockManager.Lock(_Locks);
        _Armed = true;
    }

    void OnDisable()
    {
        if (!_Armed) return;
        InputBlockManager.Unlock(_Locks);
        _Armed = false;
    }

    void OnDestroy() => OnDisable();
}
