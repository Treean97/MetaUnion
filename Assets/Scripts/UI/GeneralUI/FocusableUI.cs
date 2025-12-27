using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class FocusableUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    IItemDataProvider _ItemDataProvider;
    public static Action<InfoDataSO> OnPointerEnterFocusUI;
    public static Action OnPointerExitFocusUI;

    void Awake()
    {
        _ItemDataProvider = FindHelper.FindInterfaceInParent<IItemDataProvider>(transform, includeSelf: true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Focus();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Defocus();
    }

    public void Focus()
    {
        var data = _ItemDataProvider?.GetItemData();
        OnPointerEnterFocusUI?.Invoke(data);
    }
    
    public void Defocus()
    {
        OnPointerExitFocusUI?.Invoke();
    }
}
