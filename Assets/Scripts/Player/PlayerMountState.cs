using Controller;
using Photon.Pun;
using UnityEngine;

public class PlayerMountedState : MonoBehaviour, IMountStateApplier
{
    [Header("캐시 (비워두면 자동 탐색)")]
    [SerializeField] private MoveHandler _MoveHandler;
    [SerializeField] private CharacterController _CC;
    [SerializeField] private Rigidbody _RB;
    [SerializeField] private PhotonTransformView _PhotonTransformView;
    [SerializeField] private PlayerVisibility _Visibility;

    void Awake()
    {
        if (_MoveHandler == null) _MoveHandler = GetComponent<MoveHandler>();
        if (_CC == null) _CC = GetComponent<CharacterController>();
        if (_RB == null) _RB = GetComponent<Rigidbody>();
        if (_PhotonTransformView == null) _PhotonTransformView = GetComponent<PhotonTransformView>();
        if (_Visibility == null) _Visibility = GetComponent<PlayerVisibility>();
    }


    public void ApplyMounted(bool mounted)
    {
        if (_MoveHandler != null) _MoveHandler.enabled = !mounted;
        if (_CC != null) _CC.enabled = !mounted;

        if (_RB != null)
        {
            // rb.linearVelocity는 환경에 따라 없을 수 있음. 확실히 되는 건 velocity.
            _RB.linearVelocity = Vector3.zero;
            _RB.angularVelocity = Vector3.zero;
            _RB.isKinematic = mounted;
        }

        if (_PhotonTransformView != null) _PhotonTransformView.enabled = !mounted;
        if (_Visibility != null) _Visibility.SetVisible(!mounted);
    }
}