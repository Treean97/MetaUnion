using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    [SerializeField] SoundSO _SoundData;

    [Header("Setting")]
    [SerializeField] Transform _FootStepTransform;
    [SerializeField] string _FootStepKey;


    public void FootStep_Global()
    {
        if (!_SoundData || string.IsNullOrEmpty(_FootStepKey)) return;
        var pos = _FootStepTransform ? _FootStepTransform.position : transform.position;
        AudioManager._Inst?.PlayGlobalFromSO_RPC(_SoundData, _FootStepKey, pos);
    }
}
