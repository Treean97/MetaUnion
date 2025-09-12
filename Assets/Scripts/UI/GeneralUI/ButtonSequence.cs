using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSequence : MonoBehaviour
{
    private Button _Button;
    bool _IsRunning;

    void Awake()
    {
        _Button = GetComponent<Button>();
        _Button.onClick.AddListener(OnClick);
    }
    
    void OnDisable()
    {
        StopAllCoroutines();
        _IsRunning = false;
    }

    void OnClick()
    {
        if (!_IsRunning) StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        _IsRunning = true;

        // 이펙트 대기
        var effect = GetComponent<IButtonEffect>();
        if (effect != null)
            yield return effect.PlayRoutine(); // ← 이펙트 종료까지 정확히 대기

        _IsRunning = false;
    }
}
