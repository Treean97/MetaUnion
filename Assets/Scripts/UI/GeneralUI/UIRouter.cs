using System;
using System.Collections;
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
public interface IEmoteUI : IUI { }
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
        if (_UIs.TryGetValue(typeof(T), out var s))
        {
            var mb = s as MonoBehaviour;
            if (mb) UIFX.Show(mb.gameObject); // 연출 + 활성
            s.Show(); // 데이터 바인딩/초기화 등 내부 로직
            return true;
        }
        Debug.LogWarning($"[UIRouter] {typeof(T).Name} 미등록");
        return false;
    }

    public bool Open<T>(Action<T> init) where T : class, IUI
    {
        if (_UIs.TryGetValue(typeof(T), out var s))
        {
            // 데이터 주입
            init?.Invoke((T)s);

            // 시각적 오픈
            var mb = s as MonoBehaviour;
            if (mb) UIFX.Show(mb.gameObject);

            // UI 로직상 Show
            s.Show();
            return true;
        }
        Debug.LogWarning($"[UIRouter] {typeof(T).Name} 화면이 등록되지 않았습니다.");
        return false;
    }


    public void Close<T>() where T : class, IUI
    {
        if (_UIs.TryGetValue(typeof(T), out var s))
        {
            var mb = s as MonoBehaviour;
            s.Hide();               // 내부 정리
            if (mb) UIFX.Hide(mb.gameObject); // 연출 + 비활성
        }
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