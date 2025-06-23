using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StartBtnListener : MonoBehaviour
{
    [SerializeField] private Button _StartButton;

    void OnEnable()
    {
        // 플레이어 아이디 인풋 null 이면 비활성
        GameEvents.OnPlayerIDFieldIsNull += HandleBtnSetInteractable;
        // 로비 열리면 inactive        
        GameEvents.OnOpenLobbyUI += HandleBtnInactive;
    }
    
    void OnDisable()
    {
        GameEvents.OnPlayerIDFieldIsNull -= HandleBtnSetInteractable;
        GameEvents.OnOpenLobbyUI -= HandleBtnInactive;
    }

    // isNull이 true면 interatable는 false
    private void HandleBtnSetInteractable(bool isNull)
    {
        _StartButton.interactable = !isNull;
    }

    private void HandleBtnInactive()
    {
        _StartButton.gameObject.SetActive(false);
    } 

}
