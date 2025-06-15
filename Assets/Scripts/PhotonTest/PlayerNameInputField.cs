// using UnityEngine;
// using Photon.Pun;
// using Photon.Realtime;
// using TMPro; // TextMeshPro namespace

// namespace Com.MyCompany.MyGame
// {
//     [RequireComponent(typeof(TMP_InputField))]
//     public class PlayerNameInputField : MonoBehaviour
//     {
//         const string playerNamePrefKey = "PlayerName";

//         void Start()
//         {
//             string defaultName = string.Empty;
//             TMP_InputField _inputField = GetComponent<TMP_InputField>();

//             if (_inputField != null)
//             {
//                 if (PlayerPrefs.HasKey(playerNamePrefKey))
//                 {
//                     defaultName = PlayerPrefs.GetString(playerNamePrefKey);
//                     _inputField.text = defaultName;
//                 }
//             }

//             PhotonNetwork.NickName = defaultName;
//         }

//         public void SetPlayerName(string value)
//         {
//             if (string.IsNullOrEmpty(value))
//             {
//                 Debug.LogError("Player Name is null or empty");
//                 return;
//             }

//             PhotonNetwork.NickName = value;
//             PlayerPrefs.SetString(playerNamePrefKey, value);
//         }
//     }
// }

using UnityEngine;
using TMPro;

public class PlayerNameInputField : MonoBehaviour
{
    [SerializeField] private TMP_InputField _InputField;
    private const string _PlayerNamePrefKey = "PlayerName";

    private void Start()
    {
        string saved = PlayerPrefs.GetString(_PlayerNamePrefKey, string.Empty);
        _InputField.text = saved;

        EvaluateName(saved);
        _InputField.onValueChanged.AddListener(EvaluateName);
        _InputField.onValueChanged.AddListener(SetPlayerName);
    }

    private void EvaluateName(string tValue)
    {
        string trimmed = tValue.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            UIEvents.RaiseStartBtnInactive();
        else
            UIEvents.RaiseStartBtnActive();
    }

    public void SetPlayerName(string tValue)
    {
        string trimmed = tValue.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            Debug.LogWarning("닉네임이 비어 있습니다.");
            return;
        }

        Photon.Pun.PhotonNetwork.NickName = trimmed;
        PlayerPrefs.SetString(_PlayerNamePrefKey, trimmed);
    }
}
