using UnityEngine;


public class MinigameFish : MonoBehaviour
{
    [SerializeField] private RectTransform _Fish;
    [SerializeField] private RectTransform _ParentRect;
    [SerializeField] private float _MaxSpeed = 1f;
    private float _CurSpeed;
    [SerializeField] private float _MaxDelay;
    private float _CurMaxDelay;
    private float _CurDelay;
    // 방향
    private int _Dir;

    void OnEnable()
    {
        // 초기 랜덤 위치 설정        
        SetStartPosition();
        ChangeState();
        
    }

    void Update()
    {
        if (_CurDelay >= _CurMaxDelay)
        {
            // 방향 딜레이 수정
            ChangeState();
        }
        else
        {
            _CurDelay += Time.deltaTime;
        }

        Vector2 pos = _Fish.anchoredPosition;
        pos.x += _Dir * _CurSpeed * Time.deltaTime;
        float limit = (_ParentRect.rect.width - _Fish.rect.width) * 0.5f;
        pos.x = Mathf.Clamp(pos.x, -limit, limit);
        _Fish.anchoredPosition = pos;
    }

    void SetStartPosition()
    {
        float limit = (_ParentRect.rect.width - _Fish.rect.width) * 0.5f;
        float randomX = Random.Range(-limit, limit);
        _Fish.anchoredPosition = new Vector2(randomX, _Fish.anchoredPosition.y);
    }

    void ChangeState()
    {
        SetDelay(_MaxDelay);
        SetSpeed(_MaxSpeed);
        SetDir();
        _CurDelay = 0;
    }

    void SetDelay(float delay)
    {
        _CurMaxDelay = Random.Range(0, delay);
    }

    void SetSpeed(float speed)
    {
        _CurSpeed = Random.Range(0, speed);
    }

    void SetDir()
    {
        _Dir = Random.value < 0.5f ? 1 : -1;
    }
}
