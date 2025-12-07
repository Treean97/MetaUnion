using UnityEngine;
using TMPro;

public class PlayerIDInputManager : MonoBehaviour
{
    [SerializeField] TMP_InputField _LoginIdInput;

    void Start()
    {
        if (!_LoginIdInput)
            _LoginIdInput = GetComponent<TMP_InputField>();

        var saved = PlayerPrefs.GetString(PlayerPrefKeys.LoginIdKey, "");
        if (_LoginIdInput) _LoginIdInput.text = saved;

        if (_LoginIdInput)
        {
            _LoginIdInput.onValueChanged.AddListener(OnIdChanged);
        }
    }

    void OnDestroy()
    {
        if (_LoginIdInput)
        {
            _LoginIdInput.onValueChanged.RemoveListener(OnIdChanged);
        }
    }

    void OnIdChanged(string value)
    {
        PlayerPrefs.SetString(PlayerPrefKeys.LoginIdKey, value ?? "");
        PlayerPrefs.Save();
    }

    public string GetLoginID()
    {
        return _LoginIdInput ? _LoginIdInput.text.Trim() : "";
    }
}
