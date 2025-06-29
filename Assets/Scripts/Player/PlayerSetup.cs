using UnityEngine;
using Photon.Pun;
using Controller;
using Unity.Cinemachine;


public class PlayerSetup : MonoBehaviourPun
{
    public static GameObject _LocalPlayer;

    [SerializeField] private GameObject _PlayerCameraPrefab;

    private void Start()
    {
        if (!photonView.IsMine) return;


        _LocalPlayer = gameObject;

        GameObject camObj = Instantiate(_PlayerCameraPrefab);
        ThirdPersonCamera thirdPersonCamera = camObj.GetComponent<ThirdPersonCamera>();

        // 내 Transform을 카메라에 연결
        thirdPersonCamera.SetTarget(transform);

        // 플레이어 스크립트 → 카메라 바인딩
        var input = GetComponent<PlayerInput>();
        if (input != null)
        {
            input.BindCamera(thirdPersonCamera);
        }
            

        Debug.Log("PlayerCamera가 내 플레이어에 연결되었습니다.");
    }
}
