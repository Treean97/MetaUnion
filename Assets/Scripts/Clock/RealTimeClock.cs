using System;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;

public class RealTimeClock : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Text _TimeText;
    [SerializeField] string _TimeFormat = "hh:mm";

    CultureInfo _Culture;

    void OnEnable()
    {
        _Culture = CultureInfo.CurrentCulture;   // 로컬 문화권 고정
        StartCoroutine(CoClock());
    }

    void OnDisable() => StopAllCoroutines();

    IEnumerator CoClock()
    {
        while (true)
        {
            var now = DateTime.Now;

            // AM/PM(오전/오후) + 시:분
            string designator = now.ToString("tt", _Culture);
            if (_TimeText)
                _TimeText.text = $"{designator} {now.ToString(_TimeFormat, _Culture)}";

            // 다음 '분' 경계까지 대기
            var nextMinute = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes(1);
            var wait = (float)(nextMinute - DateTime.Now).TotalSeconds;
            if (wait < 0.05f) wait = 0.05f; // 안전 마진
            yield return new WaitForSecondsRealtime(wait);
        }
    }
}
