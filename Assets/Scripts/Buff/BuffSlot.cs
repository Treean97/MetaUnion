using UnityEngine;
using UnityEngine.UI;

public class BuffSlot : MonoBehaviour
{
    [SerializeField] Image _Icon;
    [SerializeField] Image _Duration;

    private float duration;
    private float startTime;

    public void SetSlot(BuffDataSO buffData)
    {
        _Icon.sprite = buffData.Icon;
        duration = buffData.Duration;
        startTime = Time.time;

        // 쿨다운 오버레이 초기 상태
        _Duration.fillAmount = 1f;
    }

    public bool UpdateSlot()
    {
        float elapsed = Time.time - startTime;
        float remaining = duration - elapsed;
        if (remaining <= 0f)
            return false;

        // Radial Fill Amount 조절 (1→0)
        _Duration.fillAmount = remaining / duration;
        return true;
    }
}
