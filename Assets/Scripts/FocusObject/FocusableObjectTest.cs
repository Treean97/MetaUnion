using UnityEngine;

public class FocusableObjectTest : MonoBehaviour, IFocusable
{
    [SerializeField]
    ObjectInfoSO _ObjInfo;

    public void OnFocus()
    {
        GameEvents.RaiseFocus(_ObjInfo); // 자신이 이벤트 발신
    }

    public void OnDefocus()
    {
        GameEvents.RaiseDefocus();
    }

    public ObjectInfoSO GetObjectInfo()
    {
        return _ObjInfo;
    }

}
