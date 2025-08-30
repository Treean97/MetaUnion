using System;
using System.Collections.Generic;
using UnityEngine;


#region UI 인터페이스

public interface ICustomizeUI : IUI { }
public interface IShopUI : IUI { }
public interface ISlotMachineUI : IUI { }
public interface IVendingMachineUI : IUI { }
public interface IRouletteUI : IUI { }
public interface IPlayerListUI : IUI { }
public interface IInventoryUI : IUI { }
public interface IFishingUI : IUI { }

#endregion


#region UI 라우터

public class UIRouter : MonoBehaviour
{
    public static UIRouter _Inst { get; private set; }
    readonly Dictionary<Type, IUI> _UIs = new();

    void Awake()
    {
        if (_Inst != null) { Destroy(gameObject); return; }
        _Inst = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterAs<T>(IUI ui) where T : class, IUI
    => _UIs[typeof(T)] = ui;


    public void UnregisterAs<T>(IUI screen) where T : class, IUI
    {
        var key = typeof(T);
        if (_UIs.TryGetValue(key, out var cur) && ReferenceEquals(cur, screen))
            _UIs.Remove(key);
    }


    public bool Open<T>() where T : class, IUI
    {
        if (_UIs.TryGetValue(typeof(T), out var s)) { s.Show(); return true; }
        Debug.LogWarning($"[UIRouter] {typeof(T).Name} 화면이 등록되지 않았습니다.");
        return false;
    }

    public void Close<T>() where T : class, IUI
    {
        if (_UIs.TryGetValue(typeof(T), out var s)) s.Hide();
    }

    public bool Toggle<T>(bool? force = null) where T : class, IUI
    {
        if (!_UIs.TryGetValue(typeof(T), out var s))
        {
            Debug.LogWarning($"[UIRouter] {typeof(T).Name} 화면 미등록");
            return false;
        }

        if (force.HasValue)
        {
            if (force.Value) s.Show(); else s.Hide();
            return s.IsOpen;
        }

        if (s.IsOpen) s.Hide(); else s.Show();
        return s.IsOpen;
    }
}
#endregion