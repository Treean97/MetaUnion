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
        // 모든 클라이언트에서 실행될 사운드 재생 로직
        _SoundData.TryGet(_FootStepKey, out var entry);
        if (entry == null) { Debug.LogWarning($"[Audio] 키를 찾지 못함: {key}"); return; }

        // AudioManager를 통해 로컬에서 사운드 재생
        AudioManager._Inst?.PlayLocalFromSO(_SoundData, key, _FootStepTransform.position);
    }


}
