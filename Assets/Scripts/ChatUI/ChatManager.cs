using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class ChatManager : MonoBehaviourPun
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField _ChatInputField;
    [SerializeField] private Transform _ChatContent;
    [SerializeField] private GameObject _ChatMessagePrefab;
    [SerializeField] private ScrollRect _ScrollRect;

    private void Start()
    {
        _ChatInputField.onSubmit.AddListener(SendChatMessage);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!string.IsNullOrWhiteSpace(_ChatInputField.text))
            {
                SendChatMessage(_ChatInputField.text);
            }
        }
    }

    public void SendChatMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        Debug.Log("Sender Nickname : " + PhotonNetwork.NickName);

        photonView.RPC(nameof(ReceiveChatMessage), RpcTarget.All, PhotonNetwork.NickName, message);

        _ChatInputField.text = string.Empty;
        _ChatInputField.ActivateInputField(); // 다시 입력 대기
    }

    [PunRPC]
    private void ReceiveChatMessage(string sender, string message)
    {
        GameObject chatItem = Instantiate(_ChatMessagePrefab, _ChatContent);
        TMP_Text text = chatItem.GetComponentInChildren<TMP_Text>();
        text.text = $"<b>{sender}</b> : {message}";

        Canvas.ForceUpdateCanvases(); // UI 강제 갱신
        _ScrollRect.verticalNormalizedPosition = 0f; // 스크롤 맨 아래로
    }
}
