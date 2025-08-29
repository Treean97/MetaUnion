using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager _Inst { get; private set; } 
    public bool _IsShown { get; private set; }

    int _UICount;
    bool _Manual;

    void Awake()
    {
        if (_Inst != null) { Destroy(gameObject); return; }
        _Inst = this;
        DontDestroyOnLoad(gameObject);
        Apply();
    }

    public static void Toggle()
    {
        // UI가 켜져있으면 무시
        if (_Inst._UICount != 0)
        {
            return;
        }

        _Inst._Manual = ! _Inst._Manual;
        _Inst.Apply();
    }

    public static void PushUI()
    {
        if (_Inst == null) return;
        _Inst._UICount++; _Inst.Apply();
    }

    public static void PopUI()
    {
        if (_Inst == null) return;
        if (_Inst._UICount > 0) _Inst._UICount--;
        _Inst.Apply();
    }

    void Apply()
    {
        bool show = _Manual || _UICount > 0;

        if (_IsShown != show)
        {
            _IsShown = show;
        }
        
        Cursor.visible   = show;
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
