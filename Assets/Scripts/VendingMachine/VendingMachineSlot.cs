using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class VendingMachineSlot : MonoBehaviour
{
    [SerializeField] Button _SlotBtn;
    [SerializeField] Image _Icon;    

    private ItemDataSO _ItemDataSO;

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
        GameEvents.RaiseRequestPurchaseItem(_ItemDataSO.ID, 1, _ItemDataSO.Price);
    }
}
