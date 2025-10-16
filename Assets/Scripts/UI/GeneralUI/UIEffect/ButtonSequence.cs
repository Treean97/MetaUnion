using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSequence : MonoBehaviour
{
    bool _IsRunning;

    Button _Button;

    void Awake()
    {
        _Button = GetComponent<Button>();
    }
    
    void OnEnable()
    {
        _Button.onClick.AddListener(OnClick);
    }

    void OnDisable()
    {
        _Button.onClick.RemoveListener(OnClick);
        StopAllCoroutines();
        _IsRunning = false;
    }

    void OnClick()
    {
        if (_IsRunning) return;
        StartCoroutine(RunSequence());
    }

    public IEnumerator RunSequence()
    {
        _IsRunning = true;

        // 이펙트 대기
        var effect = GetComponent<IButtonEffect>();
        if (effect != null)
            yield return effect.PlayRoutine(); // ← 이펙트 종료까지 정확히 대기

        var runner = GetComponent<ButtonActionRunner>();
        if (runner) runner.Run();

        _IsRunning = false;
    }
}
