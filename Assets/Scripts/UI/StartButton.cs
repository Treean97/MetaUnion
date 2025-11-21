using PlayFab;
using UnityEngine;

public class StartButton : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] PlayfabLoginManager _LoginManager;

    // Start 버튼에서 호출할 함수
    public void OnClickStart()
    {
        if (_LoginManager == null)
        {
            Debug.LogWarning("[StartButton] PlayfabLoginManager 참조가 없습니다.");
            return;
        }

        // 아직 PlayFab 로그인이 안 된 상태 → 로그인 UI 열기
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            // 로그인/회원가입 UI 보여주기
            _LoginManager.ShowLoginUI();
            return;
        }
        else
        {
            // 이미 로그인된 상태면 바로 로비 진입 처리
            _LoginManager.SkipLoginAndEnterLobby();    
        }

        
    }
}
