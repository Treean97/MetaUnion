using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CircleRevealOverlay : MonoBehaviour
{
    [SerializeField] float _OpenDuration = 1.5f;
    [SerializeField] float _CloseDuration = 1.5f;

    const float _TargetRadius = 0.5f;
    static readonly int _ID_Radius = Shader.PropertyToID("_Radius");

    Image _Image;
    Material _Mat;
    Sequence _Seq;

    void Awake()
    {
        _Image = GetComponent<Image>();
        _Mat = Instantiate(_Image.material);
        _Image.material = _Mat;
        _Mat.SetFloat(_ID_Radius, 0f);
        _Image.enabled = false;
    }

    void OnDestroy()
    {
        if (_Seq != null && _Seq.IsActive()) _Seq.Kill();
        if (_Mat) Destroy(_Mat);
    }

    public Tween Open()
    {
        _Seq?.Kill();
        _Image.enabled = true;
        _Mat.SetFloat(_ID_Radius, 0f);

        _Seq = DOTween.Sequence()
            .Append(DOTween.To(GetRadius, SetRadius, _TargetRadius, Mathf.Max(0.01f, _OpenDuration)).SetEase(Ease.OutCubic))
            .OnComplete(() => _Image.enabled = false);
        return _Seq;
    }

    public Tween Close()
    {
        _Seq?.Kill();
        _Image.enabled = true;

        _Seq = DOTween.Sequence()
            .Append(DOTween.To(GetRadius, SetRadius, 0f, Mathf.Max(0.01f, _CloseDuration)).SetEase(Ease.InCubic));
        return _Seq;
    }

    float GetRadius() => _Mat.GetFloat(_ID_Radius);
    void SetRadius(float r) => _Mat.SetFloat(_ID_Radius, r);

    public void SetVisible(bool v) { if (_Image) _Image.enabled = v; }
}
