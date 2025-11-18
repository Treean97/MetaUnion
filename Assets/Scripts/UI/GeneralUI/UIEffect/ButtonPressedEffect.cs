using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonPressedEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    Button _Button;
    RectTransform _ButtonRect;

    [SerializeField] float _PressOffsetRatio = 0.05f;

    RectTransform[] _Children;
    Vector2[] _OriginalPositions;

    void Awake()
    {
        _Button = GetComponent<Button>();
        _ButtonRect = GetComponent<RectTransform>();
    }

    void Start()
    {
        int count = transform.childCount;
        _Children = new RectTransform[count];
        _OriginalPositions = new Vector2[count];

        for (int i = 0; i < count; i++)
        {
            _Children[i] = transform.GetChild(i).GetComponent<RectTransform>();
            _OriginalPositions[i] = _Children[i].anchoredPosition;
        }
    }

    void OnDisable()
    {
        // 진행 중 트윈 정리 + 원위치
        if (_Children != null)
        {
            for (int i = 0; i < _Children.Length; i++)
            {
                _Children[i]?.DOKill();
                if (_Children[i]) _Children[i].anchoredPosition = _OriginalPositions[i];
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_Children == null || _OriginalPositions == null) return;

        float offsetY = -_ButtonRect.rect.height * _PressOffsetRatio;

        for (int i = 0; i < _Children.Length; i++)
        {
            if (!_Children[i]) continue;
            _Children[i].DOKill();
            // 기존과 동일하게 "즉시" 반응: duration 0f
            _Children[i].DOAnchorPos(_OriginalPositions[i] + new Vector2(0f, offsetY), 0f, true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)  => ResetPositions();

    void ResetPositions()
    {
        if (_Children == null || _OriginalPositions == null) return;

        for (int i = 0; i < _Children.Length; i++)
        {
            if (!_Children[i]) continue;
            _Children[i].DOKill();
            // 즉시 원위치 복귀(기존과 동일)
            _Children[i].DOAnchorPos(_OriginalPositions[i], 0f, true);
        }
    }
}
