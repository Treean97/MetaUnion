using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayfabManager : MonoBehaviour
{
    [Header("SignUp UI")]
    [SerializeField] TMP_InputField _SignUpIdInput;
    [SerializeField] TMP_InputField _SignUpPwInput;
    [SerializeField] Button _SignUpBtn;

    [Header("Login UI")]
    [SerializeField] TMP_InputField _LoginIdInput;
    [SerializeField] TMP_InputField _LoginPwInput;
    [SerializeField] Button _LoginBtn;

    [Header("Result UI")]
    [SerializeField] TMP_Text _StatusText;

    void OnEnable()
    {
        if (_SignUpBtn) _SignUpBtn.onClick.AddListener(OnClickSignUp);
        if (_LoginBtn)  _LoginBtn.onClick.AddListener(OnClickLogin);
    }

    void OnDisable()
    {
        if (_SignUpBtn) _SignUpBtn.onClick.RemoveListener(OnClickSignUp);
        if (_LoginBtn)  _LoginBtn.onClick.RemoveListener(OnClickLogin);
    }

    // ---------------- 회원가입 ----------------
    void OnClickSignUp()
    {
        string id = _SignUpIdInput ? _SignUpIdInput.text.Trim() : "";
        string pw = _SignUpPwInput ? _SignUpPwInput.text : "";

        // 간단 검증 (PlayFab 기본 정책: 비번 6자 이상 권장)
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

        PlayFabClientAPI.RegisterPlayFabUser(req, OnSignUpSuccess, OnSignUpError);
    }

    void OnSignUpSuccess(RegisterPlayFabUserResult res)
    {
        _SignUpBtn.interactable = true;
        SetStatus($"회원가입 성공. PlayFabId={res.PlayFabId}");
        // 필요 시: 회원가입 후 자동 로그인 상태(세션) 이미 부여됨
    }

    void OnSignUpError(PlayFabError err)
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

    // ---------------- 로그인 ----------------
    void OnClickLogin()
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

        PlayFabClientAPI.LoginWithPlayFab(req, OnLoginSuccess, OnLoginError);
    }

    void OnLoginSuccess(LoginResult res)
    {
        _LoginBtn.interactable = true;
        SetStatus($"로그인 성공. PlayFabId={res.PlayFabId}");
        // TODO: 성공 후 로비 이동 등 후속 처리
    }

    void OnLoginError(PlayFabError err)
    {
        _LoginBtn.interactable = true;

        // 대표적인 로그인 실패: 잘못된 아이디/비번
        if (err.Error == PlayFabErrorCode.InvalidParams || err.Error == PlayFabErrorCode.InvalidUsernameOrPassword || err.Error == PlayFabErrorCode.AccountNotFound)
        {
            SetStatus("아이디 또는 비밀번호가 올바르지 않습니다.");
            return;
        }

        SetStatus($"로그인 실패: {err.Error} / {err.ErrorMessage}");
    }

    // ---------------- 공통 ----------------
    void SetStatus(string msg)
    {
        if (_StatusText) _StatusText.text = msg;
        Debug.Log(msg);
    }
}
