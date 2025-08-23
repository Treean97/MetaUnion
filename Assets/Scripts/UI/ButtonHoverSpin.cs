using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class ButtonHoverSpin : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] float _HoverScale = 1.08f;
    [SerializeField] float _ScaleTime  = 0.10f;

    [Header("Click Effect")]
    [SerializeField] float _RotateAngle = 22f;
    [SerializeField] float _RotateTime  = 0.25f;
    [SerializeField] int   _Shakes      = 6;

    RectTransform _rt;
    Vector3 _baseScale;
    float _baseZ;
    Coroutine _scaleCo;

    void Awake()
    {
        _rt = (RectTransform)transform;
        _baseScale = _rt.localScale;
        _baseZ = _rt.localEulerAngles.z;
    }

    void OnDisable()
    {
        if (_scaleCo != null) StopCoroutine(_scaleCo);
        _rt.localScale = _baseScale;
        var e = _rt.localEulerAngles; e.z = _baseZ; _rt.localEulerAngles = e;
    }

    public void OnPointerEnter(PointerEventData e) => StartScale(_baseScale * _HoverScale);
    public void OnPointerExit (PointerEventData e) => StartScale(_baseScale);

    public IEnumerator ClickEffect()
    {
        yield return CoShakeZ(_RotateAngle, _RotateTime, _Shakes);
    }

    void StartScale(Vector3 target)
    {
        if (_scaleCo != null) StopCoroutine(_scaleCo);
        _scaleCo = StartCoroutine(CoScaleTo(target, _ScaleTime));
    }

    IEnumerator CoScaleTo(Vector3 target, float duration)
    {
        Vector3 start = _rt.localScale;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            float eased = 1f - (1f - u) * (1f - u); // ease-out
            _rt.localScale = Vector3.LerpUnclamped(start, target, eased);
            yield return null;
        }
        _rt.localScale = target;
    }

    IEnumerator CoShakeZ(float amplitude, float duration, int shakes)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            float angle = Mathf.Sin(u * shakes * Mathf.PI * 2f) * amplitude * (1f - u);
            var e = _rt.localEulerAngles; e.z = _baseZ + angle; _rt.localEulerAngles = e;
            yield return null;
        }
        var end = _rt.localEulerAngles; end.z = _baseZ; _rt.localEulerAngles = end;
    }
}
