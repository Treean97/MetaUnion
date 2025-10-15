using UnityEngine;

public class VendingMachineObject : MonoBehaviour, IInteractable
{
    [SerializeField]    
    InfoDataSO _ObjInfo;

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
        UIRouter._Inst.Open<IVendingMachineUI>();
        OnDefocus();
        Debug.Log("Interact");
    }

    public InfoDataSO GetObjectInfo()
    {
        return _ObjInfo;
    }
}
