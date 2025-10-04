using TMPro;
using UnityEngine;

public class NameplateSpawner : MonoBehaviour
{
    [SerializeField] private Transform _NameplateTransform;
    [SerializeField] private GameObject _NameplatePrefab;

    GameObject _NameplateObj;
    INameplate _INameplate;
    INameplateVisibility _Visibility;

    void Awake()
    {
        if (!_NameplateTransform) _NameplateTransform = transform;
        _INameplate = GetComponent<INameplate>();
        _Visibility = GetComponent<INameplateVisibility>();
    }

    void Start()
    {
        if (!_NameplatePrefab) return;

        _NameplateObj = Instantiate(_NameplatePrefab, _NameplateTransform);
        _NameplateObj.transform.localPosition = Vector3.zero;
        _NameplateObj.transform.localRotation = Quaternion.identity;

        var label = _NameplateObj.GetComponentInChildren<TMP_Text>(true);
        if (label) label.text = _INameplate?.GetDisplayName() ?? gameObject.name;

        var display = _NameplateObj.GetComponent<NameplateDisplay>();
        if (display) display.SetDisplay();

        if (_Visibility?.HideForLocal() == true) _NameplateObj.SetActive(false);
    }

    void OnDestroy()
    {
        if (_NameplateObj) Destroy(_NameplateObj);
    }


}
