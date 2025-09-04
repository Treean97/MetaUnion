using System;
using TMPro;
using UnityEngine;

public class DamagePop : MonoBehaviour
{
    [SerializeField] TMP_Text _DamageText;

    [SerializeField] private float _LifeTime;
    [SerializeField] private Vector2 _HorizSpeedRange = new Vector2(0.8f, 1.6f);
    [SerializeField] private Vector2 _UpSpeedRange = new Vector2(1.6f, 2.4f);
    [SerializeField] private float _Gravity = 9.8f;

    [SerializeField]
    private AnimationCurve _ScaleCurve =
    AnimationCurve.EaseInOut(0f, 0.6f, 0.2f, 1.0f); // 튕기듯 커졌다가 유지

    private Vector3 _BaseScale;

    private Camera _Cam;
    private Vector3 _Vel;
    private float _Life;
    private Color _Color;
    private Action<DamagePop> _OnRecycle;

    void Awake()
    {
        if (!_DamageText) _DamageText = GetComponentInChildren<TMP_Text>(true);
        _Cam = Camera.main;

        _BaseScale = transform.localScale;
        // 주의: TMP 머티리얼을 "TextMeshPro/Distance Field Overlay"로 설정해야 3D 오브젝트에 가려지지 않습니다.
    }

    private Color RandomReadableColor()
    {
        float h = UnityEngine.Random.value;
        float s = UnityEngine.Random.Range(0.75f, 1f);
        float v = UnityEngine.Random.Range(0.90f, 1f);
        return Color.HSVToRGB(h, s, v);
    }

    public void Play(Vector3 worldPos, int damage, Action<DamagePop> OnRecycle)
    {
        _OnRecycle = OnRecycle;

        transform.position = worldPos;

        // 디버그
        Debug.Log($"Damage : {damage}");

        _DamageText.text = damage.ToString();

        _Color = RandomReadableColor();
        _Color.a = 1f;
        _DamageText.color = _Color;


        if (!_Cam) _Cam = Camera.main;
        var right = _Cam ? _Cam.transform.right : Vector3.right;
        var up = _Cam ? _Cam.transform.up : Vector3.up;

        // 카메라 평면 기준 약간의 랜덤 방향 + 위로 발사
        var dir = (right * UnityEngine.Random.Range(-1f, 1f) + up * UnityEngine.Random.Range(0.2f, 0.8f)).normalized;
        var _HorizRan = UnityEngine.Random.Range(_HorizSpeedRange[0], _HorizSpeedRange[1]);
        var _UpSpeedRan = UnityEngine.Random.Range(_UpSpeedRange[0], _UpSpeedRange[1]);
        _Vel = dir * _HorizRan + Vector3.up * _UpSpeedRan;

        transform.localScale = _BaseScale * _ScaleCurve.Evaluate(0f);

        _Life = 0f;
        gameObject.SetActive(true);
    }


    void Update()
    {        
        _Life += Time.deltaTime;

        if (_Life >= _LifeTime)
        {
            _OnRecycle?.Invoke(this);
            return;
        }

        // 포물선
        _Vel += Vector3.down * _Gravity * Time.deltaTime;
        transform.position += _Vel * Time.deltaTime;

        if (_Cam)
        {
            Vector3 toCam = _Cam.transform.position - transform.position;
            if (toCam.sqrMagnitude > 1e-6f)
                transform.rotation = Quaternion.LookRotation(toCam, _Cam.transform.up);
        }

        // 진행도 (0~1)
        float t = Mathf.Clamp01(_Life / _LifeTime);

        // ★ 스케일 커브 적용
        float s = _ScaleCurve.Evaluate(t);
        transform.localScale = _BaseScale * s;

        // 페이드아웃 (기존 로직 유지)
        var c = _Color; 
        c.a = 1f - t;
        _DamageText.color = c;
    }

}
