using System;
using UnityEngine;
using UnityEngine.UI;

public class VendingMachineUIManager : MonoBehaviour, IVendingMachineUI
{
    [SerializeField] ItemDataPoolSO _VendingMachineItemDataPoolSO;
    [SerializeField] Transform _Contents;    
    [SerializeField] GameObject _SlotPrefab;

    public bool IsOpen => gameObject.activeSelf;

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

    public void Show() { }

    public void Hide() { }
}

