using System;
using System.Collections.Generic;
using UnityEngine;


#region UI 인터페이스

public interface ICustomizeUI : IUI { }
public interface IChatUI : IUI { }
public interface IShopUI : IUI { }
public interface ISlotMachineUI : IUI { }
public interface IVendingMachineUI : IUI { }
public interface IRouletteUI : IUI { }
public interface IPlayerListUI : IUI { }
public interface IInventoryUI : IUI { }
public interface IFishingUI : IUI { }
public interface ISetAmountUI : IUI
{
    void SetUI(QuantityMode mode, ItemDataSO item);
}
public interface IDialogueUI : IUI { }

#endregion


#region UI 라우터
public class UIRouter : MonoBehaviour
{
    public static UIRouter _Inst { get; private set; }
    readonly Dictionary<Type, IUI> _UIs = new();

    void Awake()
    {
        if (_Inst == null)
        {
            _Inst = this;
            // 씬이 바뀌어도 파괴되지 않도록 설정
            DontDestroyOnLoad(this);
        }
        else
        {
            // 이미 인스턴스가 존재하면 현재 오브젝트를 파괴
            Destroy(this);
        }
    }

    public void RegisterAs<T>(IUI ui) where T : class, IUI
    {
        _UIs[typeof(T)] = ui;
    }

    public void UnregisterAs<T>(IUI ui) where T : class, IUI
    {
        var key = typeof(T);
        if (_UIs.TryGetValue(key, out var cur) && ReferenceEquals(cur, ui))
            _UIs.Remove(key);
    }


    public bool Open<T>() where T : class, IUI
    {
        if (_UIs.TryGetValue(typeof(T), out var s)) { s.Show(); return true; }
        Debug.LogWarning($"[UIRouter] {typeof(T).Name} 화면이 등록되지 않았습니다.");
        return false;
    }

    public bool Open<T>(Action<T> init) where T : class, IUI
    {
        if (_UIs.TryGetValue(typeof(T), out var s))
        {
            init?.Invoke((T)s); // 여기서 SetUI 호출
            s.Show();
            return true;
        }
        Debug.LogWarning($"[UIRouter] {typeof(T).Name} not registered");
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
        Debug.Log("토글 완료");
        return s.IsOpen;
    }
}
#endregion