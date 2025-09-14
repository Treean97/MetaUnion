using UnityEngine;

public class UISoundOnOpenClose : MonoBehaviour
{
    void OnEnable()  { if (AudioManager._Inst) AudioManager._Inst.PlayUIPop(); }
    void OnDisable() { if (AudioManager._Inst) AudioManager._Inst.PlayUIClose(); }
}
