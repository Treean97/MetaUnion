using System;
using System.Collections;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayfabLoginManager : MonoBehaviour
{
    [Header("SignUp UI")]
    [SerializeField] GameObject _SignUpUI;
    [SerializeField] TMP_InputField _SignUpIdInput;
    [SerializeField] TMP_InputField _SignUpPwInput;
    [SerializeField] Button _SignUpBtn;

    [Header("Login UI")]
    [SerializeField] GameObject _LoginUI;
    [SerializeField] TMP_InputField _LoginIdInput;
    [SerializeField] TMP_InputField _LoginPwInput;
    [SerializeField] Button _LoginBtn;

    [Header("Result UI")]
    [SerializeField] TMP_Text _StatusText;

    [Header("Lobby UI")]
    [SerializeField] LobbyUIManager _LobbyUI;

    [Header("LogOut UI")]
    [SerializeField] Button _LogoutBtn;

    public static event Action OnLoginSuccess;
    Coroutine _StatusRoutine;

    void OnEnable()
    {
        if (_SignUpBtn) _SignUpBtn.onClick.AddListener(ClickSignUp);
        if (_LoginBtn)  _LoginBtn.onClick.AddListener(ClickLogin);
        if (_LogoutBtn) _LogoutBtn.onClick.AddListener(Logout);
    }

    void OnDisable()
    {
        if (_SignUpBtn) _SignUpBtn.onClick.RemoveListener(ClickSignUp);
        if (_LoginBtn)  _LoginBtn.onClick.RemoveListener(ClickLogin);
        if (_LogoutBtn) _LogoutBtn.onClick.RemoveListener(Logout);
    }

    #region 외부호출
    public void ShowLoginUI()
    {
        if (_LoginUI) UIFX.Show(_LoginUI);
    }

    public void SkipLoginAndEnterLobby()
    {
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            SetStatus("PlayFab 세션이 없습니다. 다시 로그인해주세요.");
            ShowLoginUI();
            return;
        }

        HandleLoginCompleted();
    }
    #endregion

    #region 회원가입
    void ClickSignUp()
    {
        string id = _SignUpIdInput ? _SignUpIdInput.text.Trim() : "";
        string pw = _SignUpPwInput ? _SignUpPwInput.text : "";

        // 검증
        if (string.IsNullOrEmpty(id))
        {
            SetStatus("아이디를 입력하세요.");
            return;
        }
        if (string.IsNullOrEmpty(pw) || pw.Length < 6)
        {
            SetStatus("비밀번호는 6자 이상이어야 합니다.");
            return;
        }

        _SignUpBtn.interactable = false;
        SetStatus("회원가입 중...");

        var req = new RegisterPlayFabUserRequest
        {
            Username = id,
            Password = pw,
            RequireBothUsernameAndEmail = false // Username만으로 가입 허용
        };

        PlayFabClientAPI.RegisterPlayFabUser(req, SignUpSuccess, SignUpError);
    }

    void SignUpSuccess(RegisterPlayFabUserResult res)
    {
        _SignUpBtn.interactable = true;
        SetStatus($"회원가입 성공.");
    }

    void SignUpError(PlayFabError err)
    {
        _SignUpBtn.interactable = true;

        // 대표적인 중복 케이스: UsernameNotAvailable
        if (err.Error == PlayFabErrorCode.UsernameNotAvailable)
        {
            SetStatus("이미 사용 중인 아이디입니다.");
            return;
        }

        // 그 외 에러 원문 노출(개발 중에는 원문 보는 게 정확함)
        SetStatus($"회원가입 실패: {err.Error} / {err.ErrorMessage}");
    }
    #endregion
    #region 로그인
    void ClickLogin()
    {
        string id = _LoginIdInput ? _LoginIdInput.text.Trim() : "";
        string pw = _LoginPwInput ? _LoginPwInput.text : "";

        if (string.IsNullOrEmpty(id))
        {
            SetStatus("아이디를 입력하세요.");
            return;
        }
        if (string.IsNullOrEmpty(pw))
        {
            SetStatus("비밀번호를 입력하세요.");
            return;
        }

        _LoginBtn.interactable = false;
        SetStatus("로그인 중...");

        var req = new LoginWithPlayFabRequest
        {
            Username = id,
            Password = pw,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetUserAccountInfo = true
            }
        };

        PlayFabClientAPI.LoginWithPlayFab(req, LoginSuccess, LoginError);
    }

    void LoginSuccess(LoginResult res)
    {
        _LoginBtn.interactable = true;
        SetStatus($"로그인 성공.");

        string id = _LoginIdInput ? _LoginIdInput.text.Trim() : "";

        // PlayFab DisplayName 로드/초기화
        NicknameManager._Inst.InitializeNickname(id);

        HandleLoginCompleted();
    }

    public void HandleLoginCompleted()
    {
        UIFX.Hide(_SignUpUI);
        UIFX.Hide(_LoginUI);
        UIFX.Show(_LogoutBtn.gameObject);
        if (_LobbyUI) UIFX.Show(_LobbyUI.gameObject);
        Launcher._Inst.Connect();
        OnLoginSuccess?.Invoke();
    }

    void LoginError(PlayFabError err)
    {
        _LoginBtn.interactable = true;

        // 잘못된 아이디,비밀번호
        if (err.Error == PlayFabErrorCode.InvalidParams || err.Error == PlayFabErrorCode.InvalidUsernameOrPassword || err.Error == PlayFabErrorCode.AccountNotFound)
        {
            SetStatus("아이디 또는 비밀번호가 올바르지 않습니다.");
            return;
        }

        SetStatus($"로그인 실패: {err.Error} / {err.ErrorMessage}");
    }

    public void Logout()
    {
        // PlayFab 세션 삭제
        PlayFabClientAPI.ForgetAllCredentials();

        // 2Photon 쪽은 연결은 유지하고, 방/로비만 정리
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        // 비밀번호 입력 창 초기화
        if (_LoginPwInput) _LoginPwInput.text = string.Empty;

        // UI 전환
        if (_LobbyUI) UIFX.Hide(_LobbyUI.gameObject);
        if (_LoginUI) UIFX.Show(_LoginUI);
        if (_SignUpUI) UIFX.Hide(_SignUpUI);
        if (_LogoutBtn) UIFX.Hide(_LogoutBtn.gameObject);

        SetStatus("로그아웃되었습니다. 다시 로그인해주세요.");
    }

    #endregion
    // 공통
    void SetStatus(string msg, float duration = 3f)
    {
        Debug.Log(msg);

        if (_StatusText == null)
            return;

        // 이전에 돌던 코루틴이 있으면 정지
        if (_StatusRoutine != null)
        {
            StopCoroutine(_StatusRoutine);
            _StatusRoutine = null;
        }

        // 텍스트 표시
        _StatusText.gameObject.SetActive(true);
        _StatusText.text = msg;

        // duration이 0 이하이면 계속 표시 (자동 숨김 없음)
        if (duration > 0f)
        {
            _StatusRoutine = StartCoroutine(HideStatusAfter(duration));
        }
    }

    IEnumerator HideStatusAfter(float t)
    {
        yield return new WaitForSeconds(t);

        if (_StatusText != null)
            _StatusText.gameObject.SetActive(false);

        _StatusRoutine = null;
    }
}
