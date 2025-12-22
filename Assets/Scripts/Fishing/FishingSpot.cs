using UnityEngine;

public class FishingSpot : MonoBehaviour, IInteractable
{
    [SerializeField]
    InfoDataSO _ObjInfo;

    public void OnFocus()
    {
        GameEvents.RaiseFocus(_ObjInfo);
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
