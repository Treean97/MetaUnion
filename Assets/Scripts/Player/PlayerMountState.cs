using Controller;
using Photon.Pun;
using UnityEngine;

public class PlayerMountedState : MonoBehaviour, IMountStateApplier
{
    [Header("캐시 (비워두면 자동 탐색)")]
    [SerializeField] private MoveHandler _MoveHandler;
    [SerializeField] private CharacterController _CC;
    [SerializeField] private PhotonTransformView _PhotonTransformView;
    [SerializeField] private PlayerVisibility _Visibility;
    [SerializeField] private Animator _Animator;

    void Awake()
    {
        if (_MoveHandler == null) _MoveHandler = GetComponent<MoveHandler>();
        if (_CC == null) _CC = GetComponent<CharacterController>();
        if (_PhotonTransformView == null) _PhotonTransformView = GetComponent<PhotonTransformView>();
        if (_Visibility == null) _Visibility = GetComponent<PlayerVisibility>();
        if (_Animator == null) _Animator = GetComponent<Animator>();
    }


    public void ApplyMounted(bool mounted)
    {
        if (_MoveHandler != null) _MoveHandler.enabled = !mounted;
        if (_CC != null) _CC.enabled = !mounted;
        if (_PhotonTransformView != null) _PhotonTransformView.enabled = !mounted;
        if (_Visibility != null) _Visibility.SetVisible(!mounted);
        if (_Animator != null) _Animator.enabled = !mounted;
    }
}