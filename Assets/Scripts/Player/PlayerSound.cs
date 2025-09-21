using Photon.Pun;
using UnityEngine;

public class PlayerSound : MonoBehaviourPunCallbacks
{
    private PhotonView _PV;

    void Awake()
    {
        _PV = GetComponent<PhotonView>();
    }

    public void PlayLocal(string key)
    {
        if (string.IsNullOrEmpty(key)) return;

        // 로컬 즉시 재생
        AudioManager._Inst?.PlayLocalByKey(key);
    }

    public void PlayGlobal(string key)
    {
        if (string.IsNullOrEmpty(key)) return;

        Vector3 pos = this.transform.position;

        // 로컬 즉시 재생
        AudioManager._Inst?.PlayLocalByKey(key, pos);

        // 원격 전파(내 오브젝트일 때만, All 금지)
        if (_PV && _PV.IsMine)
            _PV.RPC(nameof(RPC_PlaySoundGlobal), RpcTarget.Others, key, pos);
    }

    [PunRPC]
    void RPC_PlaySoundGlobal(string key, Vector3 worldPos)
    {
        // ✅ 원격에서도 동일하게 들리도록, 전달받은 값만 사용
        AudioManager._Inst?.PlayLocalByKey(key, worldPos);
    }

}
