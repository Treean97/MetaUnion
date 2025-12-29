using System;
using System.Collections.Generic;

public interface IFocusUIBlockFlag
{
    bool Value { get; }              // true면 포커스UI 숨김
    event Action Changed;            // 값 바뀌면 호출
    string DebugName { get; }        // 디버그용
}

public static class FocusUIBlockAuto
{
    private static readonly Dictionary<IFocusUIBlockFlag, IDisposable> _Tokens = new();
    private static readonly Dictionary<IFocusUIBlockFlag, Action> _Handlers = new();

    public static void Register(IFocusUIBlockFlag flag)
    {
        if (flag == null) return;
        if (_Handlers.ContainsKey(flag)) return;

        Action handler = () => Refresh(flag);
        _Handlers[flag] = handler;
        flag.Changed += handler;

        Refresh(flag);
    }

    public static void Unregister(IFocusUIBlockFlag flag)
    {
        if (flag == null) return;

        if (_Handlers.TryGetValue(flag, out var handler))
        {
            flag.Changed -= handler;
            _Handlers.Remove(flag);
        }

        if (_Tokens.TryGetValue(flag, out var token))
        {
            token?.Dispose();
            _Tokens.Remove(flag);
        }
    }

    private static void Refresh(IFocusUIBlockFlag flag)
    {
        bool shouldBlock = flag.Value;

        if (shouldBlock)
        {
            if (_Tokens.ContainsKey(flag)) return;
            _Tokens[flag] = FocusUIBlockManager.AcquireBlockToken(flag.DebugName);
            return;
        }

        if (_Tokens.TryGetValue(flag, out var token))
        {
            token?.Dispose();
            _Tokens.Remove(flag);
        }
    }
}
