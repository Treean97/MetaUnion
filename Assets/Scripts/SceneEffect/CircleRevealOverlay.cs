using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CircleRevealOverlay : MonoBehaviour
{
    [SerializeField] float _OpenDuration  = 1.5f; // 0   -> 0.5 까지 걸리는 시간
    [SerializeField] float _CloseDuration = 1.5f; // 0.5 -> 0   까지 걸리는 시간

    const float R_MIN = 0f;
    const float R_MAX = 0.5f; // 셰이더 Range(0,0.5)에 맞춤
    static readonly int _ID_Radius = Shader.PropertyToID("_Radius");

    Image _Image;
    Material _Mat;
    Tween _Tween; // Sequence 대신 Tween 하나면 충분

    void Awake()
    {
        _Image = GetComponent<Image>();

        // Source Image 없으면 생성(렌더링 보장)
        if (_Image.sprite == null)
            _Image.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,1,1), new Vector2(0.5f,0.5f), 1f);

        _Mat = Instantiate(_Image.material);
        _Image.material = _Mat;

        _Mat.SetFloat(_ID_Radius, R_MIN);
        _Image.enabled = false;
    }

    void OnDestroy()
    {
        _Tween?.Kill();
        if (_Mat) Destroy(_Mat);
    }

    public Tween Open()
    {
        _Tween?.Kill();
        _Image.enabled = true;

        // 시작 0 → 0.5
        _Mat.SetFloat(_ID_Radius, R_MIN);
        _Image.SetMaterialDirty();

        _Tween = _Mat
            .DOFloat(R_MAX, _ID_Radius, Mathf.Max(0.01f, _OpenDuration)) // ★ Material.DOFloat 사용
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)                     // 타임스케일 0에서도 재생
            .OnUpdate(() => _Image.SetMaterialDirty()) // 즉시 리드로우
            .OnComplete(() => _Image.enabled = false);

        return _Tween;
    }

    public Tween Close()
    {
        _Tween?.Kill();
        _Image.enabled = true;

        // 시작 0.5 → 0
        _Mat.SetFloat(_ID_Radius, R_MAX);
        _Image.SetMaterialDirty();

        _Tween = _Mat
            .DOFloat(R_MIN, _ID_Radius, Mathf.Max(0.01f, _CloseDuration))
            .SetEase(Ease.InCubic)
            .SetUpdate(true)
            .OnUpdate(() => _Image.SetMaterialDirty());

        return _Tween;
    }

    public void SetVisible(bool visible)
    {
        if (!_Image) return;
        _Image.enabled = visible;
    }
}
