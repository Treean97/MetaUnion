using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;


public class PlayerNameInputManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _InputField;
    [SerializeField] private Button _LoginButton;
    private const string _PlayerNamePrefKey = "PlayerName";

    private void Start()
    {
        string saved = PlayerPrefs.GetString(_PlayerNamePrefKey, string.Empty);
        _InputField.text = saved;

        EvaluateName(saved);
        SetPlayerName(saved);
        
        _InputField.onValueChanged.AddListener(EvaluateName);
        _InputField.onValueChanged.AddListener(SetPlayerName);
    }

    private void EvaluateName(string tValue)
    {
        bool vaild = !string.IsNullOrWhiteSpace(tValue?.Trim());
        if (_LoginButton) _LoginButton.interactable = vaild;
    }

    public void SetPlayerName(string tValue)
    {
        string trimmed = tValue.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            Debug.LogWarning("닉네임이 비어 있습니다.");
            return;
        }

        PhotonNetwork.NickName = trimmed;
        PlayerPrefs.SetString(_PlayerNamePrefKey, trimmed);
    }
}
