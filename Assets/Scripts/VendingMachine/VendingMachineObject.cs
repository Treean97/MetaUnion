using UnityEngine;

public class VendingMachineObject : MonoBehaviour
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
        GameEvents.RaiseRequestOpenVendingMachineUI();
        OnDefocus();
        Debug.Log("Interact");
    }

    public ItemInfoSO GetObjectInfo()
    {
        return _ObjInfo;
    }
}
