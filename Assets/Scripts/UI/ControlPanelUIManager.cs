using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlPanelUIManager : MonoBehaviour
{
    [Header("Set")]
    [SerializeField] private GameObject _LobbyUI;

    [Header("Input")]
    [SerializeField] private TMP_InputField _PlayerIDInput;

    [Header("Button")]
    [SerializeField] private Button _LoginBtn;

    void OnEnable()
    {
        _PlayerIDInput.onValueChanged.AddListener(HandlePlayerIDChanged);
        _LoginBtn.onClick.AddListener(OnClickLoginButton);
        HandlePlayerIDChanged(_PlayerIDInput.text);
    }

    void OnDisable()
    {        
        _LoginBtn.onClick.RemoveListener(OnClickLoginButton);
        _PlayerIDInput.onValueChanged.RemoveListener(HandlePlayerIDChanged);
    }

    void OnClickLoginButton()
    {
        StartCoroutine(LoginButtonSequence());
    }

    IEnumerator LoginButtonSequence()
    {
        _LoginBtn.interactable = false;

        var effect = _LoginBtn.GetComponent<ButtonSpinEffect>();

        if (effect != null)
        {
            yield return StartCoroutine(effect.PlayRoutine()); // 이펙트 완료까지 대기
        }            

        // 이펙트 끝난 뒤에 전환
        _LobbyUI.SetActive(true);
        Launcher._Inst.Connect();
        gameObject.SetActive(false);
    }
    
    void HandlePlayerIDChanged(string value)
    {
        _LoginBtn.interactable = !string.IsNullOrWhiteSpace(value);
    }
}
