using System.Collections;
using DG.Tweening;
using UnityEngine;

public class UIPopEffect : MonoBehaviour
{
   [Header("Show (켜질 때)")]
    [SerializeField] float _ShowFromScale = 0.85f;
    [SerializeField] float _ShowDuration  = 0.15f;
    [SerializeField] Ease  _ShowEase      = Ease.OutQuad;

    [Header("Hide (꺼질 때)")]
    [SerializeField] float _PopScale      = 1.05f;   // 잠깐 커짐
    [SerializeField] float _HideToScale   = 0.80f;   // 작아지면서 종료
    [SerializeField] float _HideDuration  = 0.12f;
    [SerializeField] Ease  _HideEase      = Ease.InQuad;

    RectTransform _RT;
    Tween _Tween; // 진행 중 트윈(Sequence 포함)

    void Awake()
    {
        _RT = (RectTransform)transform;
    }

    void OnDisable()
    {
        // 비활성화 시 트윈 정리(풀링/메모리 누수 방지)
        _Tween?.Kill();
        _Tween = null;
    }

    public void PlayShow()
    {
        _Tween?.Kill();
        _RT.localScale = Vector3.one * _ShowFromScale;

        // 켜질 때는 SetActive가 이미 되어 있어야 보임
        // 외부(OpenUIAction)에서 SetActive(true) 후 PlayShow 호출 권장

        var seq = DOTween.Sequence();
        seq.Join(_RT.DOScale(1f, _ShowDuration).SetEase(_ShowEase));
        _Tween = seq;
    }

    /// <summary>
    /// 닫기: 연출이 끝난 후 gameObject.SetActive(false)
    /// 반드시 코루틴에서 WaitForCompletion으로 대기할 것.
    /// </summary>
    public IEnumerator PlayHide()
    {
        _Tween?.Kill();

        // 잠깐 커졌다가(_PopScale) → 작아지면서(_HideToScale) 페이드아웃
        var seq = DOTween.Sequence();

        // 팝업 업(짧게)
        seq.Append(_RT.DOScale(_PopScale, 0.06f).SetEase(Ease.OutQuad));

        // 축소 + 페이드아웃
        seq.Append(_RT.DOScale(_HideToScale, _HideDuration).SetEase(_HideEase));

        _Tween = seq;
        yield return _Tween.WaitForCompletion();

        gameObject.SetActive(false);
    }
}
