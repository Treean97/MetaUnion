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

    /// <summary>페이드 인(열기). 내부적으로 Image.enabled = false 상태로 마무리.</summary>
    public Tween FadeIn()
    {
        if (_Circle == null) return DOVirtual.DelayedCall(0f, () => { });
        // Open은 radius 0 → _TargetRadius로 확장 후 Image.enabled=false
        return _Circle.Open();
    }

    /// <summary>페이드 아웃(닫기). 화면을 덮는 검정 상태로 수렴(Image.enabled=true 유지).</summary>
    public Tween FadeOut()
    {
        if (_Circle == null) return DOVirtual.DelayedCall(0f, () => { });
        // Close는 radius -> 0 으로 수렴, Image.enabled=true 유지(검정)
        return _Circle.Close();
    }

    /// <summary>즉시 표시/숨김(애니메이션 없음). 로딩 UI 노출용 등.</summary>
    public void SetOverlayVisible(bool visible)
    {
        if (_Circle != null) _Circle.SetVisible(visible);
    }

    /// <summary>페이드 아웃 종료까지 대기하고, 검정 유지(다음 쪽에서 FadeIn 호출 가정).</summary>
    public IEnumerator WaitDuringFadeOut()
    {
        if (_Circle == null) yield break;
        // 닫기 전에 반드시 보이게(안 보이는 상태에서 트윈해도 화면에 안 드러남)
        _Circle.SetVisible(true);
        yield return FadeOut().WaitForCompletion();
        // 여기서 검정 유지 (FadeIn은 호출 측에서)
    }
}
