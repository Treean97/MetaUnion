using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class FocusableUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    IItemDataProvider itemDataProvider;
    public static Action<InfoDataSO> OnPointerEnterFocusUI;
    public static Action OnPointerExitFocusUI;

    void Awake()
    {
        itemDataProvider = GetComponent<IItemDataProvider>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var data = itemDataProvider?.GetItemData();
        OnPointerEnterFocusUI?.Invoke(data);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitFocusUI?.Invoke();
    }
}
