using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ButtonSequence))]
[RequireComponent(typeof(Button))]
public class ButtonSpinEffect : MonoBehaviour, IButtonEffect
{
    [Header("Click Effect")]
    [SerializeField] float _RotateAngle = 22f;
    [SerializeField] float _RotateTime = 0.25f;
    [SerializeField] int _Shakes = 6;

    private RectTransform _Rt;
    private float _BaseZ;
    Coroutine _ShakeCo;

    void Awake()
    {
        _Rt = (RectTransform)transform;
        _BaseZ = _Rt.localEulerAngles.z;
    }

    void OnDisable()
    {
        if (_ShakeCo != null) StopCoroutine(_ShakeCo);
        var e = _Rt.localEulerAngles; e.z = _BaseZ; _Rt.localEulerAngles = e;
    }

    public IEnumerator PlayRoutine()
    {
        yield return CoShakeZ(_RotateAngle, _RotateTime, _Shakes);
    }

    IEnumerator CoShakeZ(float amplitude, float duration, int shakes)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / duration);
            float angle = Mathf.Sin(u * shakes * Mathf.PI * 2f) * amplitude * (1f - u);
            var e = _Rt.localEulerAngles; e.z = _BaseZ + angle; _Rt.localEulerAngles = e;
            yield return null;
        }
        var end = _Rt.localEulerAngles; end.z = _BaseZ; _Rt.localEulerAngles = end;
    }
}
