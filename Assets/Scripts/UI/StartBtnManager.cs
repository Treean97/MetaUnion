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
        var sequence = _StartBtn.GetComponent<ButtonSequence>();
        if (sequence)
        {
            yield return sequence.RunSequence();
        }

        _ControlUI.SetActive(true);
        gameObject.SetActive(false);
    }

}
