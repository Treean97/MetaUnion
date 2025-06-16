using UnityEngine;
using Photon.Pun;
using Controller;
using Unity.Cinemachine;


public class PlayerSetup : MonoBehaviourPun
{
    [SerializeField] private GameObject _PlayerCameraPrefab;

    private void Start()
    {
        if (!photonView.IsMine)
            return;

        GameObject camObj = Instantiate(_PlayerCameraPrefab);
        ThirdPersonCamera thirdPersonCamera = camObj.GetComponent<ThirdPersonCamera>();

        // ③ 내 Transform을 카메라에 연결
        thirdPersonCamera.SetTarget(transform);

        Debug.Log("PlayerCamera가 내 플레이어에 연결되었습니다.");
    }
}
