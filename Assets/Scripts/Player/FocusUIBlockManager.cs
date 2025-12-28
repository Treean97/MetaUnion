using System;
using System.Collections.Generic;
using UnityEngine;

public static class FocusUIBlockManager
{
    // true: 숨김, false: 표시
    public static event Action<bool> OnFocusUIBlockStatus;

    // 토큰 기반 관리
    private static int _NextTokenId = 1;
    private static readonly HashSet<int> _ActiveTokens = new HashSet<int>();

    public static bool IsFocusUIBlocked => _ActiveTokens.Count > 0;

    // 토큰 획득
    public static IDisposable AcquireBlockToken(string ownerDebug = null)
    {
        int id = _NextTokenId++;
        _ActiveTokens.Add(id);

        if (_ActiveTokens.Count == 1)
        {
            Debug.Log($"FocusUIBlock ON (token:{id}) owner:{ownerDebug}");
            OnFocusUIBlockStatus?.Invoke(true);
        }
        else
        {
            Debug.Log($"FocusUIBlock + (token:{id}) owner:{ownerDebug} count:{_ActiveTokens.Count}");
        }

        return new BlockToken(id, ownerDebug);
    }

    private static void ReleaseToken(int id, string ownerDebug)
    {
        if (!_ActiveTokens.Remove(id))
        {
            // 이미 해제됐거나 잘못된 토큰
            Debug.LogWarning($"FocusUIBlock token release ignored (token:{id}) owner:{ownerDebug}");
            return;
        }

        if (_ActiveTokens.Count == 0)
        {
            Debug.Log($"FocusUIBlock OFF (token:{id}) owner:{ownerDebug}");
            OnFocusUIBlockStatus?.Invoke(false);
        }
        else
        {
            Debug.Log($"FocusUIBlock - (token:{id}) owner:{ownerDebug} count:{_ActiveTokens.Count}");
        }
    }

    private sealed class BlockToken : IDisposable
    {
        private readonly int _Id;
        private readonly string _OwnerDebug;
        private bool _Disposed;

        public BlockToken(int id, string ownerDebug)
        {
            _Id = id;
            _OwnerDebug = ownerDebug;
        }

        public void Dispose()
        {
            if (_Disposed) return;
            _Disposed = true;
            ReleaseToken(_Id, _OwnerDebug);
        }
    }
}
