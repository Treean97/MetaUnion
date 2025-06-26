using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System.Collections;

public class ChatManager : MonoBehaviourPun
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField _ChatInputField;
    [SerializeField] private Transform _ChatContent;
    [SerializeField] private GameObject _ChatMessagePrefab;
    [SerializeField] private ScrollRect _ScrollRect;

    private void Start()
    {
        // 인풋 필드 포커스/디포커스 이벤트
        _ChatInputField.onSelect.AddListener((_) => GameEvents.RaiseUIIsRunning(true));
        _ChatInputField.onDeselect.AddListener((_) => GameEvents.RaiseUIIsRunning(false));
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && !_ChatInputField.isFocused)
        {
            SendChatMessage(_ChatInputField.text);

            //StartCoroutine(ActivateInputNextFrame());
        }
    }

    IEnumerator ActivateInputNextFrame()
    {
        yield return null;
        _ChatInputField.ActivateInputField();
        _ChatInputField.MoveTextEnd(false);
    }

    public void SendChatMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        Debug.Log("Sender Nickname : " + PhotonNetwork.NickName);

        photonView.RPC(nameof(ReceiveChatMessage), RpcTarget.All, PhotonNetwork.NickName, message);

        _ChatInputField.ActivateInputField();
        _ChatInputField.text = string.Empty;
    }

    [PunRPC]
    private void ReceiveChatMessage(string sender, string message)
    {
        GameObject chatItem = Instantiate(_ChatMessagePrefab, _ChatContent);
        TMP_Text text = chatItem.GetComponent<TMP_Text>();
        text.text = $"<b>{sender}</b> : {message}";
        
        StartCoroutine(ScrollUpdate());
    }

    IEnumerator ScrollUpdate()
    {
        yield return null;
        _ScrollRect.verticalNormalizedPosition = 0f;
    }
}
