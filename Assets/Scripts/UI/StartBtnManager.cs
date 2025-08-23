using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StartBtnManager : MonoBehaviour
{
    [SerializeField] private Button _StartBtn;
    private ButtonHoverSpin _Effect;

    void Awake()
    {
        _Effect = gameObject.GetComponent<ButtonHoverSpin>();
    }

    void OnEnable()
    {
        _StartBtn.onClick.AddListener(OnClickStart);
    }

    void Start()
    {
        // 씬 실행 시 active
        gameObject.SetActive(true);
    }
    
    void OnClickStart()
    {
        StartCoroutine(CoClickSequence());
    }

    IEnumerator CoClickSequence()
    {
        _StartBtn.interactable = false;

        if (_Effect != null)
            yield return StartCoroutine(_Effect.ClickEffect()); // 이펙트 완료까지 대기

        // 이펙트 끝난 뒤에 전환
        GameEvents.RaiseRequestOpenLobbyUI();
        GameEvents.RaiseConnect();

        gameObject.SetActive(false);
    }


}
