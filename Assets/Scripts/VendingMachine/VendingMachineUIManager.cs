using UnityEngine;
using UnityEngine.UI;

public class VendingMachineUIManager : MonoBehaviour
{
    [SerializeField] ItemDataPoolSO _VendingMachineItemDataPoolSO;
    [SerializeField] Transform _Contents;
    [SerializeField] GameObject _SlotPrefab;
    [SerializeField] Button _CloseBtn;

    void Awake()
    {
        _CloseBtn.onClick.AddListener(OnClickCloseBtn);
    }

    void Start()
    {
        // 슬롯 생성 및 할당
        for (int i = 0; i < _VendingMachineItemDataPoolSO.GetItemCount(); i++)
        {
            GameObject obj = Instantiate(_SlotPrefab, _Contents);
            obj.GetComponent<VendingMachineSlot>().
            SetSlot(_VendingMachineItemDataPoolSO.GetItemAt(i));            
        }
        
    }


    void OnClickCloseBtn()
    {
        gameObject.SetActive(false);
    }
}
