using Photon.Pun;
using TMPro;
using UnityEngine;

public class NameplateSpawner : MonoBehaviour
{
    [SerializeField] private Transform _NameplateTransform;
    [SerializeField] private GameObject _NameplatePrefab;

    private GameObject _nameplate;

    void Start()
    {
        var photonView = GetComponent<PhotonView>();

        _nameplate = Instantiate(_NameplatePrefab, _NameplateTransform);
        _nameplate.transform.localPosition = Vector3.zero;
        _nameplate.transform.localRotation = Quaternion.identity;

        var label = _nameplate.GetComponentInChildren<TMP_Text>(true);
        if (label) label.text = photonView.Owner.NickName;

        // 각 클라이언트에서 카메라를 찾게 함
        var display = _nameplate.GetComponent<NameplateDisplay>();
        if (display) display.SetDisplay(_NameplateTransform);

        // 자신 이름표 숨김
        if (photonView.IsMine) _nameplate.SetActive(false);
    }

    void OnDestroy()
    {
        if (_nameplate)
        {
            Destroy(_nameplate);
        }
    }


}
