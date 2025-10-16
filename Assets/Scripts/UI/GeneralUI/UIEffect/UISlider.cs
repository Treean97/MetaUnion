using DG.Tweening;
using UnityEngine;

public class UISlider : MonoBehaviour
{
    [SerializeField] private RectTransform _Target;
    [SerializeField] private Vector2 _OpenPosition;
    [SerializeField] private Vector2 _ClosePosition;
    [SerializeField] private float _Duration = 0.5f;

    [Header("Tween Options")]
    [SerializeField] private Ease _Ease = Ease.OutCubic;
    [SerializeField] private bool _UnscaledTime = true; // 기존처럼 Time.unscaledDeltaTime 기준

    private bool _IsOpen;
    public bool IsOpen => _IsOpen;

    Tween _MoveTween;
    public bool _IsAnimating =>
        _MoveTween != null && _MoveTween.IsActive() && _MoveTween.IsPlaying();

    void Awake()
    {
        if (_Target == null) _Target = GetComponent<RectTransform>();
    }

    void OnDisable()
    {
        _MoveTween?.Kill();
        _MoveTween = null;
    }

    public void Show()
    {
        if (_IsAnimating) return;
        MoveTo(_OpenPosition, true);
    }

    public void Hide()
    {
        if (_IsAnimating) return;
        MoveTo(_ClosePosition, false);
    }

    public void Toggle()
    {
        if (_IsAnimating) return;
        MoveTo(_IsOpen ? _ClosePosition : _OpenPosition, !_IsOpen);
    }

    void MoveTo(Vector2 end, bool open)
    {
        _MoveTween?.Kill(); // 항상 현재 위치에서 시작(DoTween이 자동으로 현재값을 사용)
        _MoveTween = _Target
            .DOAnchorPos(end, _Duration)
            .SetEase(_Ease)
            .SetUpdate(_UnscaledTime)
            .OnComplete(() =>
            {
                _IsOpen = open;
                _MoveTween = null;
            });
    }
}
