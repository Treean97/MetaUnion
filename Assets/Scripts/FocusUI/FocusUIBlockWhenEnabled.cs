using System;
using UnityEngine;

public class FocusUIBlockGuard : MonoBehaviour
{
    private IDisposable _Token;

    void OnEnable()
    {
        if (_Token != null) return;
        _Token = FocusUIBlockManager.AcquireBlockToken(gameObject.name);
    }

    void OnDisable()
    {
        _Token?.Dispose();
        _Token = null;
    }

    void OnDestroy() => OnDisable(); // 누수 방지
}
