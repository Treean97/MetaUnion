using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
public interface IColorUI : IUI
{
    void SetUI(CustomizeItemSO item);
}
public interface IChangeNameUI : IUI
{
    void SetUI(string lastNickName, InventoryItem inventoryItem, GameObject user);
}
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

    bool TryOpen(IUI ui)
    {
        if (ui == null) return false;

        // 이미 열려 있으면 무시
        if (ui.IsOpen) return true;

        if (ui is MonoBehaviour mb)
        {
            UIFX.Show(mb.gameObject);
        }

        ui.Show();
        return true;
    }


    public bool Open<T>() where T : class, IUI
    {
        if (_UIs.TryGetValue(typeof(T), out var s))
        {
            return TryOpen(s);
        }

        Debug.LogWarning($"[UIRouter] {typeof(T).Name} 미등록");
        return false;
    }

    public bool Open<T>(Action<T> init) where T : class, IUI
    {
        if (_UIs.TryGetValue(typeof(T), out var s))
        {
            var ui = (T)s;

            // 데이터 주입은 항상 허용 (같은 UI라도 다른 데이터로 띄울 수 있으니까)
            init?.Invoke(ui);

            return TryOpen(ui);
        }

        Debug.LogWarning($"[UIRouter] {typeof(T).Name} 화면이 등록되지 않았습니다.");
        return false;
    }


    public void Close<T>() where T : class, IUI
    {
        if (_UIs.TryGetValue(typeof(T), out var s))
        {
            var mb = s as MonoBehaviour;
            s.Hide(); // 내부 정리
            if (mb) UIFX.Hide(mb.gameObject); // 연출 + 비활성
        }
    }


    public bool Toggle<T>() where T : class, IUI
    {
        if (!_UIs.TryGetValue(typeof(T), out var s))
        {
            Debug.LogWarning($"[UIRouter] {typeof(T).Name} 화면 미등록");
            return false;
        }

        if (s.IsOpen)
            Close<T>();
        else
            TryOpen(s);

        return s.IsOpen;
    }
}
#endregion

#region NPC 대화 UI 연결

public enum DialogueUIKey
{
    Shop,
    SlotMachine,
    VendingMachine,
    Roulette,
}

public static class UIRouterDialogueExtensions
{
    public static bool Open(this UIRouter router, DialogueUIKey key)
    {
        if (router == null)
        {
            Debug.LogWarning("[UIRouter] 인스턴스 없음");
            return false;
        }

        switch (key)
        {
            case DialogueUIKey.Shop:
                return router.Open<IShopUI>();

            case DialogueUIKey.SlotMachine:
                return router.Open<ISlotMachineUI>();

            case DialogueUIKey.VendingMachine:
                return router.Open<IVendingMachineUI>();

            case DialogueUIKey.Roulette:
                return router.Open<IRouletteUI>();

            default:
                Debug.LogWarning($"[UIRouter] 매핑되지 않은 DialogueUIKey: {key}");
                return false;
        }
    }
}
#endregion