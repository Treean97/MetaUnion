using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ButtonSequence))]
[RequireComponent(typeof(Button))]
public class ButtonSpinEffect : MonoBehaviour, IButtonEffect
{
    [Header("Click Effect")]
    [SerializeField] float _RotateAngle = 22f;
    [SerializeField] float _RotateTime  = 0.25f;
    [SerializeField] int   _Shakes      = 6;

    RectTransform _Rt;
    float _BaseZ;
    Tween _ShakeTween;

    void Awake()
    {
        _Rt = (RectTransform)transform;
        _BaseZ = _Rt.localEulerAngles.z;
    }

    void OnDisable()
    {
        _ShakeTween?.Kill();
        var e = _Rt.localEulerAngles; e.z = _BaseZ; _Rt.localEulerAngles = e;
    }

    public IEnumerator PlayRoutine()
    {
        // 사인 파형 * 감쇠(1 - u)를 그대로 재현
        _ShakeTween?.Kill();

        _ShakeTween = DOVirtual.Float(0f, 1f, _RotateTime, u =>
        {
            u = Mathf.Clamp01(u);
            float angle = Mathf.Sin(u * _Shakes * Mathf.PI * 2f) * _RotateAngle * (1f - u);
            var e = _Rt.localEulerAngles; e.z = _BaseZ + angle; _Rt.localEulerAngles = e;
        })
        .SetEase(Ease.Linear)
        .SetUpdate(true) // 기존과 동일: unscaledDeltaTime 기반
        .OnComplete(() =>
        {
            var e = _Rt.localEulerAngles; e.z = _BaseZ; _Rt.localEulerAngles = e;
        });

        yield return _ShakeTween.WaitForCompletion();
    }
}
