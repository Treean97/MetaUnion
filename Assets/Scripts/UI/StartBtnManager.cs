using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StartBtnManager : MonoBehaviour
{
    [SerializeField] private Button _StartBtn;
    [SerializeField] private GameObject _ControlUI;

    void OnEnable()
    {
        _StartBtn.interactable = true;
        _StartBtn.onClick.AddListener(OnClickStart);
    }

    void Start()
    {
        // 씬 실행 시 active
        gameObject.SetActive(true);
    }

    void OnClickStart()
    {
        StartCoroutine(StartButtonSequence());
    }
    
    IEnumerator StartButtonSequence()
    {
        _StartBtn.interactable = false;

        // 버튼 이펙트 가져오기
        var effect = _StartBtn.GetComponent<ButtonHoverSpin>();

        if (effect != null)
            yield return StartCoroutine(effect.ClickEffect());

        _ControlUI.SetActive(true);
        gameObject.SetActive(false);
    }

}
