using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Image))]
public class Confetti : MonoBehaviour
{
    RectTransform _RT;
    Image _Image;
    Sequence _Seq;

    void Awake()
    {
        _RT = (RectTransform)transform;
        _Image = GetComponent<Image>();
        if (_Image) _Image.raycastTarget = false;
    }

    public void Setup(
        Color color,
        float lifetime,
        Vector2 velocity,
        Vector3 angularVelocity,
        float startScale,
        float fadeOutTime)
    {
        // 초기값
        _RT.localScale = Vector3.one * startScale;
        _Image.color = color;

        // 목표값 계산
        var startPos = _RT.anchoredPosition;
        var endPos   = startPos + velocity * lifetime;

        var startEuler = _RT.localEulerAngles;
        var endEuler   = startEuler + angularVelocity * lifetime;

        // 시퀀스
        _Seq = DOTween.Sequence();

        // 이동
        _Seq.Join(_RT.DOAnchorPos(endPos, lifetime).SetEase(Ease.Linear));

        // 회전
        _Seq.Join(_RT.DORotate(endEuler, lifetime, RotateMode.FastBeyond360).SetEase(Ease.Linear));

        // 페이드(마지막 구간만)
        if (fadeOutTime > 0f)
        {
            float t = Mathf.Min(fadeOutTime, lifetime);
            _Seq.Insert(lifetime - t, _Image.DOFade(0f, t).SetEase(Ease.Linear));
        }

        // 완료 시 자기 파괴
        _Seq.OnComplete(() => Destroy(gameObject));
    }

    void OnDestroy()
    {
        if (_Seq != null && _Seq.IsActive()) _Seq.Kill();
    }
}
