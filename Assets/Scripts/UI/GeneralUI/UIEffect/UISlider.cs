using System.Collections;
using UnityEngine;

public class UISlider : MonoBehaviour
{
    [SerializeField] private RectTransform _Target;
    [SerializeField] private Vector2 _OpenPosition;
    [SerializeField] private Vector2 _ClosePosition;
    [SerializeField] private float _Duration = 0.5f;

    private bool _IsOpen;
    public bool IsOpen => _IsOpen;

    private Coroutine _CoMove;

    // 토글중
    public bool _IsAnimating => _CoMove != null;

    void Awake()
    {
        if (_Target == null) _Target = GetComponent<RectTransform>();
    }

    public void Show()
    {
        if (_IsAnimating) return;
        MoveTo(_OpenPosition, true);
    }
    public void Hide()
    {
        if (_IsAnimating) return;
        MoveTo(_ClosePosition, false);    
    }
    
    public void Toggle()
    {
        if (_IsAnimating) return;
        MoveTo(_IsOpen ? _ClosePosition : _OpenPosition, !_IsOpen);
    }

    void MoveTo(Vector2 end, bool open)
    {
        if (_CoMove != null) { StopCoroutine(_CoMove); _CoMove = null; }

        Vector2 start = _Target.anchoredPosition; // 항상 현재 위치에서 시작
        _CoMove = StartCoroutine(CoMove(start, end, open));
    }

    IEnumerator CoMove(Vector2 start, Vector2 end, bool open)
    {
        float elapsed = 0f;

        while (elapsed < _Duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _Duration);
            _Target.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }

        _Target.anchoredPosition = end;
        _IsOpen = open;
        _CoMove = null;
    }
}
