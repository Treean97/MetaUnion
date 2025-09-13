using UnityEngine;

public class UISoundOnOpenClose : MonoBehaviour
{
    void OnEnable()  { if (SoundManager._Inst) SoundManager._Inst.PlayUIPop(); }
    void OnDisable() { if (SoundManager._Inst) SoundManager._Inst.PlayUIClose(); }
}
