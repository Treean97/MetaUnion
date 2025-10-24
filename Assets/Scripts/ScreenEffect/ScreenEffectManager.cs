using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ScreenEffectManager : MonoBehaviour
{
    public static ScreenEffectManager _Inst { get; private set; }

    [Header("Effects")]
    [SerializeField] private CircleRevealOverlay _Circle;

    void Awake()
    {
        if (_Inst != null && _Inst != this) { Destroy(gameObject); return; }
        _Inst = this;
        DontDestroyOnLoad(gameObject);
    }

    // 페이드 인(열기)
    public Tween FadeIn()
    {
        if (_Circle == null) return DOVirtual.DelayedCall(0f, () => { });
        // Open은 radius 0 → _TargetRadius로 확장 후 Image.enabled=false
        return _Circle.Open();
    }

    //페이드 아웃(닫기)
    public Tween FadeOut()
    {
        if (_Circle == null) return DOVirtual.DelayedCall(0f, () => { });
        // Close는 radius -> 0 으로 수렴, Image.enabled=true 유지(검정)
        return _Circle.Close();
    }

    // 즉시 표시/숨김
    public void SetOverlayVisible(bool visible)
    {
        if (_Circle != null) _Circle.SetVisible(visible);
    }

    //페이드 아웃 종료까지 대기하고, 검정 유지
    public IEnumerator WaitDuringFadeOut()
    {
        if (_Circle == null) yield break;
        // 닫기 전에 반드시 보이게(안 보이는 상태에서 트윈해도 화면에 안 드러남)
        _Circle.SetVisible(true);
        yield return FadeOut().WaitForCompletion();
        // 여기서 검정 유지 (FadeIn은 호출 측에서)
    }
}
