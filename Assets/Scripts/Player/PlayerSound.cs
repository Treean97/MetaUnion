using Photon.Pun;
using UnityEngine;

public class PlayerSound : MonoBehaviourPunCallbacks
{
    [SerializeField] SoundSO _SoundData;

    [Header("Setting")]
    [SerializeField] Transform _FootStepTransform;
    [SerializeField] string _FootStepKey;

    private PhotonView _PV;

    void Awake()
    {
        _PV = GetComponent<PhotonView>();
    }

    public void FootStep_Global()
    {
        if (!_PV.IsMine) return;

        if (!_SoundData || string.IsNullOrEmpty(_FootStepKey)) return;
        
        var pos = _FootStepTransform ? _FootStepTransform.position : transform.position;

        _PV.RPC(nameof(RPC_PlayFootstep), RpcTarget.All, _FootStepKey, pos);
    }


    [PunRPC]
    void RPC_PlayFootstep(string key, Vector3 worldPos)
    {
        // ✅ 원격에서도 동일하게 들리도록, 전달받은 값만 사용
        AudioManager._Inst?.PlayLocalByKey(key, worldPos);
    }

}
