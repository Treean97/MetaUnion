using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VendingMachineSlot : MonoBehaviour,
IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Button _SlotBtn;
    [SerializeField] Image _Icon;    
    private ItemDataSO _ItemDataSO;

    public static event Action<ItemDataSO> OnPointerEnterVendingMachineSlot;
    public static event Action OnPointerExitVendingMachineSlot;

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterVendingMachineSlot?.Invoke(_ItemDataSO);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitVendingMachineSlot?.Invoke();
    }

    public void SetSlot(ItemDataSO itemDataSO)
    {
        _ItemDataSO = itemDataSO;
        _Icon.sprite = _ItemDataSO.Icon;

        _SlotBtn.onClick.RemoveAllListeners();
        _SlotBtn.onClick.AddListener(OnClickSlot);
    }

    void OnClickSlot()
    {
        // 테스트 amount = 1 추후 수량 설정 UI 추가
        GameEvents.RaiseRequestPurchaseItem(_ItemDataSO.ID, 1, 10000, _ItemDataSO.Price);
    }
}
