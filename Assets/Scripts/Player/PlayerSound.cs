using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(CharacterController))]
public class PlayerSound : MonoBehaviourPunCallbacks
{
    [Header("Keys")]
    [SerializeField] string _FootKey = "Footstep";
    [SerializeField] string _JumpKey = "Jump";
    [SerializeField] string _HitKey = "Hit";
    [SerializeField] string _SwingKey = "Swing";
    [SerializeField] string _FishingStartKey = "FishingStart";
    [SerializeField] string _FishingSuccessKey = "FishingSuccess";
    [SerializeField] string _FishingFailKey = "FishingFail";
    [SerializeField] string _ItemPickUpKey = "ItemPickUp";


    [Header("Setting")]
    [SerializeField] float _MinSpeedForStep = 0.15f;

    private PhotonView _PV;
    private CharacterController _CC;

    void Awake()
    {
        _PV = GetComponent<PhotonView>();
        _CC = GetComponent<CharacterController>();
    }

    public void FootStep() // 애니메이션 이벤트에서 이거 호출
    {
        if (!_CC || !_CC.isGrounded) return;

        Vector3 v = _CC.velocity; v.y = 0f;
        if (v.magnitude < _MinSpeedForStep) return;

        PlayGlobal(_FootKey);
    }

    public void JumpSound() // 점프 시작 시 애니메이션 이벤트
    {
        PlayGlobal(_JumpKey);
    }

    public void HitSound()
    {
        PlayGlobal(_HitKey);
    }

    public void SwingSound()
    {
        PlayGlobal(_SwingKey);
    }

    public void FishingStart()
    {
        PlayGlobal(_FishingStartKey);
    }

    public void FishingSuccess()
    {
        PlayGlobal(_FishingSuccessKey);
    }

    public void FishingFail()
    {
        PlayGlobal(_FishingFailKey);
    }

    public void ItemPickUpSound()
    {
        PlayGlobal(_ItemPickUpKey);
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
