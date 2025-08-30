using System;
using UnityEngine;
using UnityEngine.UI;

public class VendingMachineUIManager : MonoBehaviour, IVendingMachineUI
{
    [SerializeField] ItemDataPoolSO _VendingMachineItemDataPoolSO;
    [SerializeField] Transform _Contents;    
    [SerializeField] GameObject _SlotPrefab;
    [SerializeField] SetAmountUIManager _SetAmountUI;
    [SerializeField] Button _CloseBtn;



    void Awake()
    {
        _CloseBtn.onClick.AddListener(OnClickCloseBtn);        
    }

    void OnEnable()
    {
        _SetAmountUI.gameObject.SetActive(false);
        UIRouter._Inst.Open<IVendingMachineUI>();
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
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
