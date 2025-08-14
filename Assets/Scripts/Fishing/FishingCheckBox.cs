using UnityEngine;

public class FishingCheckBox : MonoBehaviour
{
    [SerializeField] private FishingMinigame _FishingUIManager;
    [SerializeField] private RectTransform _CheckBox;
    [SerializeField] private RectTransform _BoxLimit;
    [SerializeField] private float _Speed;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<MinigameFish>(out var fish))
        {
            FishingMinigame.RaiseFishInCheckBox();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<MinigameFish>(out var fish))
        {
            FishingMinigame.RaiseFishOutCheckBox();
        }
    }

    void Update()
    {
        float input = Input.GetAxisRaw("Horizontal");

        if (input != 0)
        {
            Vector2 pos = _CheckBox.anchoredPosition;
            pos.x += input * _Speed * Time.deltaTime;

            float limit = (_BoxLimit.rect.width - _CheckBox.rect.width) * 0.5f;
            pos.x = Mathf.Clamp(pos.x, -limit, limit);
            _CheckBox.anchoredPosition = pos;
        }
        
    }
}
