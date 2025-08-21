using UnityEngine;
using UnityEngine.UI;

public class ParallaxUI : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public RectTransform Rect; // 이동시킬 레이어(RectTransform)
        [Range(0f, 400f)]
        public float Strength = 20f; // 마우스 가장자리에 도달했을 때 레이어가 이동할 최대 픽셀량
    }

    [SerializeField] private Canvas _Canvas; // 비우면 자동 탐색
    [SerializeField] private Layer[] _Layers;
    [SerializeField] private float _Smooth = 10f; // 부드럽게 따라오기(값↑ 빠르게 수렴)
    [SerializeField] private bool _EnableParallax = true; // 패럴럭스 오프시에도 경계 클램프만 적용 가능

    private Vector2[] _BasePos; // 각 레이어의 기준 위치(앵커드 포지션)

    void Awake()
    {
        if (!_Canvas) _Canvas = GetComponentInParent<Canvas>();
        _BasePos = new Vector2[_Layers.Length];
        for (int i = 0; i < _Layers.Length; i++)
        {
            if (_Layers[i].Rect != null)
                _BasePos[i] = _Layers[i].Rect.anchoredPosition;
        }
    }

    void Update()
    {
        if (_Canvas == null || _Layers == null) return;

        // 마우스 → 캔버스 로컬 좌표 [-1,1]로 정규화
        var canvasRect = (RectTransform)_Canvas.transform;
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            _Canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _Canvas.worldCamera,
            out local
        );

        float nx = Mathf.Clamp(local.x / (canvasRect.rect.width  * 0.5f), -1f, 1f);
        float ny = Mathf.Clamp(local.y / (canvasRect.rect.height * 0.5f), -1f, 1f);
        Vector2 n = new Vector2(nx, ny);

        // 레이어 이동 + 경계 클램프
        for (int i = 0; i < _Layers.Length; i++)
        {
            var layer = _Layers[i];
            if (layer.Rect == null) continue;

            // 목표위치: 기준 + (정규화*강도)  (패럴럭스 끄면 기준만 유지)
            Vector2 target = _EnableParallax
                ? _BasePos[i] + n * layer.Strength
                : _BasePos[i];

            // 캔버스 경계 안쪽으로만 보이도록 "항상 완전 덮이게" 클램프
            target = ClampToCoverCanvas(layer.Rect, _BasePos[i], target, canvasRect);

            // 부드럽게 보간
            Vector2 cur = layer.Rect.anchoredPosition;
            float t = 1f - Mathf.Exp(-_Smooth * Time.deltaTime);
            layer.Rect.anchoredPosition = Vector2.Lerp(cur, target, t);
        }
    }

    /// <summary>
    /// 레이어의 크기가 캔버스보다 큰 만큼만 기준점에서 이동하도록 제한.
    /// 이렇게 하면 레이어의 테두리가 캔버스 안으로 절대 들어오지 않음(항상 화면을 완전 덮음).
    /// 전제: Pivot = (0.5, 0.5)
    /// </summary>
    private Vector2 ClampToCoverCanvas(RectTransform layerRect, Vector2 basePos, Vector2 target, RectTransform canvasRect)
    {
        Vector2 canvasSize = canvasRect.rect.size;
        Vector2 layerSize  = layerRect.rect.size;

        // 레이어가 캔버스보다 큰 만큼(중심 기준 반측)만 이동 허용
        float hx = Mathf.Max(0f, (layerSize.x - canvasSize.x) * 0.5f);
        float hy = Mathf.Max(0f, (layerSize.y - canvasSize.y) * 0.5f);

        if (hx <= 0f) target.x = basePos.x;
        else          target.x = Mathf.Clamp(target.x, basePos.x - hx, basePos.x + hx);

        if (hy <= 0f) target.y = basePos.y;
        else          target.y = Mathf.Clamp(target.y, basePos.y - hy, basePos.y + hy);

        return target;
    }
}
