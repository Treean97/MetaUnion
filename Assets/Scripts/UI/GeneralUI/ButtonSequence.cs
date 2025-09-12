using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSequence : MonoBehaviour
{
    bool _IsRunning;
    
    void OnDisable()
    {
        StopAllCoroutines();
        _IsRunning = false;
    }


    public IEnumerator RunSequence()
    {
        _IsRunning = true;

        // 이펙트 대기
        var effect = GetComponent<IButtonEffect>();
        if (effect != null)
            yield return effect.PlayRoutine(); // ← 이펙트 종료까지 정확히 대기

        _IsRunning = false;
    }
}
