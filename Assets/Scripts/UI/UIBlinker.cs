using UnityEngine;
using UnityEngine.UI;

public class UIBlinker : MonoBehaviour
{
    [SerializeField] float _BlinkPeriod;
    [SerializeField] float _MinAlpha = 0.25f;  // 최소 알파
    [SerializeField] float _MaxAlpha = 1.0f;   // 최대 알파
    Image _Icon;

    void Awake()
    {
        _Icon = GetComponent<Image>();        
    }

    // Update is called once per frame
    void Update()
    {
        float u = Mathf.PingPong(Time.unscaledTime / _BlinkPeriod, 1f);
        var c = _Icon.color;
        c.a = Mathf.Lerp(_MinAlpha, _MaxAlpha, u);
        _Icon.color = c;
    }
}
