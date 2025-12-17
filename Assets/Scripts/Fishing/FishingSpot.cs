using UnityEngine;

public class FishingSpot : MonoBehaviour, IInteractable
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
        UIRouter._Inst.Open<IFishingUI>();
    }

    public InfoDataSO GetObjectInfo()
    {
        return _ObjInfo;
    }
}
