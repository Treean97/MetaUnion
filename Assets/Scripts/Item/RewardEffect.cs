using System.Collections.Generic;
using UnityEngine;

public class RewardEffect : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] RectTransform _CanvasRT; // Screen Space 캔버스
    [SerializeField] RectTransform _PanelRT; // RewardUIPanel
    [SerializeField] Confetti _ConfettiPrefab; // 종이조각 프리팹(Confetti.cs 포함)

    [Header("Confetti Spawn (Screen Top Random)")]
    [SerializeField] int _ConfettiCount = 48;
    [SerializeField] float _TopYMargin = 40f; // 상단에서 약간 내려오게
    [SerializeField] float _SideXMargin = 60f; // 좌우 여백

    [Header("Confetti Motion (fixed for lifetime)")]
    [SerializeField] Vector2 _LifetimeRange = new Vector2(3f, 5f);
    [SerializeField] Vector2 _FallSpeedRange = new Vector2(600f, 950f); // +y 위, 아래로는 음수 적용
    [SerializeField] Vector2 _DriftXRange = new Vector2(-140f, 140f);
    [SerializeField] Vector2 _SpinXRange = new Vector2(-90f, 90f);
    [SerializeField] Vector2 _SpinYRange = new Vector2(-90f, 90f);
    [SerializeField] Vector2 _SpinZRange = new Vector2(180f, 540f);
    [SerializeField] Vector2 _StartScaleRange = new Vector2(0.75f, 1.15f);
    [SerializeField] float _FadeOutTime = 0.45f;

    [Header("Color (RGB Random Only)")]
    [Tooltip("RGB 랜덤 범위(0~1). 필요시 채널별로 제한 가능")]
    [SerializeField] Vector2 _RRange = new Vector2(0f, 1f);
    [SerializeField] Vector2 _GRange = new Vector2(0f, 1f);
    [SerializeField] Vector2 _BRange = new Vector2(0f, 1f);

    readonly List<GameObject> _Spawned = new();

    public void Play()
    {
        SpawnConfetti();
    }

    // --- Confetti ---
    void SpawnConfetti()
    {
        if (!_CanvasRT || !_ConfettiPrefab || !_PanelRT) return;

        Vector2 canvasSize = _CanvasRT.rect.size;
        float halfW = canvasSize.x * 0.5f;
        float halfH = canvasSize.y * 0.5f;

        float y = halfH - _TopYMargin;
        float xMin = -halfW + _SideXMargin;
        float xMax = halfW - _SideXMargin;

        for (int i = 0; i < _ConfettiCount; i++)
        {
            float x = Random.Range(xMin, xMax);

            
            RectTransform parent = (RectTransform)_PanelRT.parent;
            var confetti = Instantiate(_ConfettiPrefab, parent);
            var rt = (RectTransform)confetti.transform;
            
            rt.SetSiblingIndex(_PanelRT.GetSiblingIndex() + 1);

            rt.anchoredPosition = new Vector2(x, y);
            rt.localEulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));

            float lifetime = Random.Range(_LifetimeRange.x, _LifetimeRange.y);
            float fallSpeed = Random.Range(_FallSpeedRange.x, _FallSpeedRange.y);
            float driftX = Random.Range(_DriftXRange.x, _DriftXRange.y);

            float spinX = Random.Range(_SpinXRange.x, _SpinXRange.y);
            float spinY = Random.Range(_SpinYRange.x, _SpinYRange.y);
            float spinZ = Random.Range(_SpinZRange.x, _SpinZRange.y) * (Random.value < 0.5f ? -1f : 1f);

            float startScale = Random.Range(_StartScaleRange.x, _StartScaleRange.y);

            // RGB 랜덤(항상)
            float r = Random.Range(_RRange.x, _RRange.y);
            float g = Random.Range(_GRange.x, _GRange.y);
            float b = Random.Range(_BRange.x, _BRange.y);
            Color color = new Color(r, g, b, 1f);

            Vector2 velocity = new Vector2(driftX, -fallSpeed);
            Vector3 spinDegPerSec = new Vector3(spinX, spinY, spinZ);

            confetti.Setup(
                color,
                lifetime,
                velocity,
                spinDegPerSec,
                startScale,
                _FadeOutTime
            );

            _Spawned.Add(confetti.gameObject);
        }
    }
}
