using UnityEngine;
using UnityEngine.UI;

public class StartBtnManager : MonoBehaviour
{
    [SerializeField] private Button _StartBtn;

    void OnEnable()
    {
        _StartBtn.onClick.AddListener(() => GameEvents.RaiseRequestOpenLobbyUI());
        _StartBtn.onClick.AddListener(() => GameEvents.RaiseConnect());
    }

    void Start()
    {
        // 씬 실행 시 active
        gameObject.SetActive(true);        
    }


}
