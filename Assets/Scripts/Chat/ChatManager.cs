using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System.Collections;

public class ChatManager : MonoBehaviourPun, IChatUI
{
    [Header("UI Set")]
    [SerializeField] private UISlider _UISlider;
    [SerializeField] private TMP_InputField _ChatInputField;
    [SerializeField] private Transform _ChatContent;
    [SerializeField] private GameObject _ChatMessagePrefab;
    [SerializeField] private ScrollRect _ScrollRect;
    [SerializeField] private Image _NoticeUI;
    [SerializeField] private string _NoticeKey;

    public bool IsOpen => _UISlider != null && _UISlider.IsOpen;

    [Header("채팅 중 막을 인풋")]
    [SerializeField] private InputLock _TypingLocks =
        InputLock.Attack | InputLock.Interact | InputLock.UIHotkey | InputLock.Move;

    private bool _TypingArmed;

    private void Start()
    {
        // 인풋 필드 포커스/디포커스 이벤트
        _ChatInputField.onSelect.AddListener((_) =>
        {
            if (_TypingArmed) return;
            InputBlockManager.Lock(_TypingLocks);
            _TypingArmed = true;
        });

        _ChatInputField.onDeselect.AddListener((_) =>
        {
            if (!_TypingArmed) return;
            InputBlockManager.Unlock(_TypingLocks);
            _TypingArmed = false;
        });
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && !_ChatInputField.isFocused)
        {
            SendChatMessage(_ChatInputField.text);
        }         
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
        
        if (!IsOpen)
        {
            ShowNotice(); // 닫혀 있으면 알림 표시
        }
        
    }

    IEnumerator ScrollUpdate()
    {
        yield return null;
        _ScrollRect.verticalNormalizedPosition = 0f;
    }

    void ShowNotice()
    {
        if (_NoticeUI) _NoticeUI.gameObject.SetActive(true);
        AudioManager._Inst.PlayLocalByKey(_NoticeKey);
    }

    void HideNotice()
    {
        if (_NoticeUI) _NoticeUI.gameObject.SetActive(false);
    }

    public void Show()
    {
        if (_UISlider) _UISlider.Show();
        HideNotice();
    }

    public void Hide()
    {
        if (_UISlider) _UISlider.Hide();
    }

    public void Toggle() { if (_UISlider) _UISlider.Toggle(); }
}
