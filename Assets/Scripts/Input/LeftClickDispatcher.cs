using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class LeftClickDispatcher : MonoBehaviour
{
    public static LeftClickDispatcher _Inst { get; private set; }

    readonly List<ILeftClickConsumer> _Stack = new();

    void Awake()
    {
        if (_Inst != null && _Inst != this) { Destroy(gameObject); return; }
        _Inst = this;
    }

    // 가장 나중에 Push된 것이 우선
    public IDisposable Push(ILeftClickConsumer consumer)
    {
        if (consumer == null) return default;

        // 중복 등록 방지 + 최신 우선
        _Stack.Remove(consumer);
        _Stack.Add(consumer);

        return new PopToken(this, consumer);
    }

    void Pop(ILeftClickConsumer consumer)
    {
        if (consumer == null) return;
        _Stack.Remove(consumer);
    }

    public void Dispatch()
    {
        // Top부터 소비 시도
        for (int i = _Stack.Count - 1; i >= 0; --i)
        {
            var c = _Stack[i];
            if (c == null) { _Stack.RemoveAt(i); continue; }
            if (c.ConsumeLeftClick()) return;
        }
    }

    readonly struct PopToken : IDisposable
    {
        readonly LeftClickDispatcher _Owner;
        readonly ILeftClickConsumer _Consumer;

        public PopToken(LeftClickDispatcher owner, ILeftClickConsumer consumer)
        {
            _Owner = owner;
            _Consumer = consumer;
        }

        public void Dispose()
        {
            if (_Owner != null) _Owner.Pop(_Consumer);
        }
    }
}
