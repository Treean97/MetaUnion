using UnityEngine;
using TMPro;
using Photon.Pun;


public class PlayerIDInputManager : MonoBehaviour
{
    private TMP_InputField _IDInputField;
    private const string _PlayerNamePrefKey = "PlayerName";

    void Start()
    {
        _IDInputField = GetComponent<TMP_InputField>();

        // 저장된 닉네임 로드
        string saved = PlayerPrefs.GetString(_PlayerNamePrefKey, string.Empty);
        if (_IDInputField) _IDInputField.text = saved;

        // 초기값 적용
        ApplyNickname(saved);

        // 변경 시 즉시 저장/적용
        if (_IDInputField) _IDInputField.onValueChanged.AddListener(OnNickChanged);
    }

    void OnDestroy()
    {
        if (_IDInputField) _IDInputField.onValueChanged.RemoveListener(OnNickChanged);
    }

    // 입력값 변경 시 호출
    void OnNickChanged(string value)
    {
        ApplyNickname(value);
    }

    // 닉네임 적용 + 저장
    void ApplyNickname(string value)
    {
        string trimmed = (value ?? "").Trim();
        if (string.IsNullOrWhiteSpace(trimmed)) return;

        PhotonNetwork.NickName = trimmed;
        PlayerPrefs.SetString(_PlayerNamePrefKey, trimmed);
        PlayerPrefs.Save();
    }
}
