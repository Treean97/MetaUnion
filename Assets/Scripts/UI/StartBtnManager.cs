using UnityEngine;
using UnityEngine.UI;

public class StartBtnManager : MonoBehaviour
{

    [SerializeField] private Button _StartButton;

    void Awake()
    {
        // 플레이어 아이디 인풋 null 이면 비활성
        GameEvents.OnPlayerIDFieldIsNull += HandleBtnSetInteractable;
        // 로비 열리면 inactive        
        GameEvents.OnOpenLobbyUI += HandleBtnInactive;
    }

    void Start()
    {
        // 씬 실행 시 active
        gameObject.SetActive(true);        
    }

    void OnDestroy()
    {
        GameEvents.OnPlayerIDFieldIsNull -= HandleBtnSetInteractable;
        GameEvents.OnOpenLobbyUI -= HandleBtnInactive;
    }

    // isNull이 true면 interatable는 false
    private void HandleBtnSetInteractable(bool isNull)
    {
        _StartButton.interactable = !isNull;
    }

    
    private void HandleBtnActive()
    {
        _StartButton.gameObject.SetActive(true);
    } 

    private void HandleBtnInactive()
    {
        _StartButton.gameObject.SetActive(false);
    } 
}
