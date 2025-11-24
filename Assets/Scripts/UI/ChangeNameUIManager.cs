using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;

public class ChangeNameUIManager : MonoBehaviour, IChangeNameUI
{
    [SerializeField] TMP_InputField _NameInputField;
    [SerializeField] Button _ConfirmButton;

    InventoryItem _InventoryItem;

    public bool IsOpen => enabled;

    void Awake()
    {
        _ConfirmButton.onClick.AddListener(OnClickConfirm);
    }

    public void SetUI(string lastNickName, InventoryItem inventoryItem, GameObject user)
    {
        _InventoryItem = inventoryItem;

        _NameInputField.text = lastNickName;
        _NameInputField.Select();
        _NameInputField.ActivateInputField();
    }

    void OnClickConfirm()
    {
        var newName = (_NameInputField.text ?? "").Trim();
        if (string.IsNullOrWhiteSpace(newName))
        {
            GameEvents.RaiseShowWarning("닉네임을 입력하세요.");
            return;
        }

        // PlayFab에 DisplayName 업데이트
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = newName
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request,
            result =>
            {
                Debug.Log($"DisplayName changed to {result.DisplayName}");

                // 클라이언트(Photon + PlayerPrefs)에 적용
                NicknameManager._Inst.ApplyNickname(result.DisplayName);

                // 아이템 소비
                GameEvents.RaiseRequestItemSpend(_InventoryItem.ID, 1);

                // UI 닫기
                gameObject.SetActive(false);
            },
            error =>
            {
                Debug.LogError($"Failed to change nickname: {error.ErrorMessage}");
                GameEvents.RaiseShowWarning("닉네임 변경에 실패했습니다.");
            }
        );
    }

    public void Show() { }

    public void Hide() { }
}
