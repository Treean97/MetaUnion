using PlayFab;
using PlayFab.ClientModels;
using Photon.Pun;
using UnityEngine;

public class NicknameManager : MonoBehaviour
{
    public static NicknameManager _Inst { get; private set; }

    void Awake()
    {
        if (_Inst != null && _Inst != this)
        {
            Destroy(gameObject);
            return;
        }
        _Inst = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// PlayFab 로그인 성공 후 호출.
    /// loginId: 이번에 로그인에 사용한 ID
    /// </summary>
    public void InitializeNickname(string loginId)
    {
        var request = new GetAccountInfoRequest();

        PlayFabClientAPI.GetAccountInfo(request,
            result =>
            {
                string displayName = result.AccountInfo?.TitleInfo?.DisplayName;

                if (string.IsNullOrEmpty(displayName))
                {
                    // 최초 로그인: DisplayName 없음 → loginId로 등록
                    SetFirstTimeNickname(loginId);
                }
                else
                {
                    // 이미 닉네임 있음
                    ApplyNickname(displayName);
                }
            },
            error =>
            {
                Debug.LogError($"GetAccountInfo failed: {error.ErrorMessage}");
                // 실패하면 일단 loginId를 닉네임으로 사용
                ApplyNickname(loginId);
            }
        );
    }

    /// <summary>
    /// 최초 로그인 시 loginId를 닉네임으로 PlayFab에 저장
    /// </summary>
    void SetFirstTimeNickname(string loginId)
    {
        string trimmed = (loginId ?? "").Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            trimmed = "Player";

        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = trimmed
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request,
            result =>
            {
                Debug.Log($"First DisplayName set to {result.DisplayName}");
                ApplyNickname(result.DisplayName);
            },
            error =>
            {
                Debug.LogError($"UpdateUserTitleDisplayName (first) failed: {error.ErrorMessage}");
                // 실패해도 클라 쪽은 loginId로 사용
                ApplyNickname(trimmed);
            }
        );
    }

    /// <summary>
    /// Photon + PlayerPrefs에 닉네임 적용
    /// </summary>
    public void ApplyNickname(string nickname)
    {
        string trimmed = (nickname ?? "").Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            trimmed = "Player";

        PhotonNetwork.NickName = trimmed;

        PlayerPrefs.SetString(PlayerPrefKeys.NicknameKey, trimmed);
        PlayerPrefs.Save();

        Debug.Log($"Nickname applied: {trimmed}");
    }
}
