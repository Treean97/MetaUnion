using System;
using UnityEngine;

public static class InputBlockManager
{
    static int _Move, _Look, _Attack, _Interact, _UIHotkey;

    public static bool IsLocked(InputLock t) => t switch
    {
        InputLock.Move     => _Move > 0,
        InputLock.Look     => _Look > 0,
        InputLock.Attack   => _Attack > 0,
        InputLock.Interact => _Interact > 0,
        InputLock.UIHotkey => _UIHotkey > 0,
        _ => false
    };

    public static void Lock(InputLock locks)   => Apply(locks, +1);
    public static void Unlock(InputLock locks) => Apply(locks, -1);

    static void Apply(InputLock locks, int delta)
    {
        if ((locks & InputLock.Move) != 0)     _Move += delta;
        if ((locks & InputLock.Look) != 0)     _Look += delta;
        if ((locks & InputLock.Attack) != 0)   _Attack += delta;
        if ((locks & InputLock.Interact) != 0) _Interact += delta;
        if ((locks & InputLock.UIHotkey) != 0) _UIHotkey += delta;

        _Move     = Math.Max(0, _Move);
        _Look     = Math.Max(0, _Look);
        _Attack   = Math.Max(0, _Attack);
        _Interact = Math.Max(0, _Interact);
        _UIHotkey = Math.Max(0, _UIHotkey);
    }
}
