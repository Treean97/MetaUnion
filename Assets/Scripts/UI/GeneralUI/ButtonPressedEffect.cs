using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class ButtonPressedEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private Button _Button;
    private RectTransform _ButtonRect;
    [SerializeField] private float _PressOffsetRatio = 0.05f;

    private RectTransform[] _Children;
    private Vector2[] _OriginalPositions;


    void Awake()
    {
        _Button = gameObject.GetComponent<Button>();
        _ButtonRect = gameObject.GetComponent<RectTransform>();
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

    public void OnPointerDown(PointerEventData eventData)
    {
        float offsetY = -_ButtonRect.rect.height * _PressOffsetRatio;

        for (int i = 0; i < _Children.Length; i++)
        {
            _Children[i].anchoredPosition = _OriginalPositions[i] + new Vector2(0, offsetY);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetPositions();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetPositions();
    }

    private void ResetPositions()
    {
        if (_Children == null || _OriginalPositions == null) return;

        for (int i = 0; i < _Children.Length; i++)
        {
            _Children[i].anchoredPosition = _OriginalPositions[i];
        }
            
    }
}
