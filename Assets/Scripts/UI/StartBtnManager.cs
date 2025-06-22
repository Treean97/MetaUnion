using UnityEngine;
using UnityEngine.UI;

public class StartBtnManager : MonoBehaviour
{

    [SerializeField] private Button _StartButton;

    void Awake()
    {
        _StartButton.onClick.AddListener(() => GameEvents.RaiseRequestOpenLobbyUI());
        _StartButton.onClick.AddListener(() => GameEvents.RaiseConnect());
    }

    void Start()
    {
        // 씬 실행 시 active
        gameObject.SetActive(true);        
    }


}
