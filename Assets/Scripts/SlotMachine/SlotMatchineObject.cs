using System;
using UnityEngine;

public class SlotMatchineObject : MonoBehaviour, IInteractable
{
    [SerializeField]
    ItemInfoSO _ObjInfo;
  

    public void OnFocus()
    {
        GameEvents.RaiseFocus(_ObjInfo); // 자신이 이벤트 발신
    }

    public void OnDefocus()
    {
        GameEvents.RaiseDefocus();
    }

    public void OnInteract()
    {
        UIRouter._Inst.Open<ISlotMachineUI>();
        OnDefocus();
        Debug.Log("Interact");
    }

    public ItemInfoSO GetObjectInfo()
    {
        return _ObjInfo;
    }

}
