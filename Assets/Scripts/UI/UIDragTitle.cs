using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragTitle : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    [Header("이동시킬 패널")]
    [SerializeField] RectTransform _Target;

    Vector2 _Offset;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_Target) return;
        var parentRect = _Target.parent as RectTransform;
        if (!parentRect) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out var localPoint);

        // 시작 시점에서 마우스 위치와 패널 위치 차이 저장
        _Offset = _Target.anchoredPosition - localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_Target) return;
        var parentRect = _Target.parent as RectTransform;
        if (!parentRect) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out var localPoint);

        // 드래그 동안 패널 위치 갱신
        _Target.anchoredPosition = localPoint + _Offset;
    }
}
