using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerPWInputManager : MonoBehaviour, ISelectHandler, IDeselectHandler 
{
    TMP_InputField _PWInputField;

    IMECompositionMode _PrevIme;

    void Awake()
    {
        _PWInputField = GetComponent<TMP_InputField>();
    }

    void Reset() => _PWInputField = GetComponent<TMP_InputField>();

    void OnEnable()
    {
        if (_PWInputField) _PWInputField.onValueChanged.AddListener(FilterAscii);
    }
    void OnDisable()
    {
        if (_PWInputField) _PWInputField.onValueChanged.RemoveListener(FilterAscii);
        // 선택 중 비활성화되면 IME 복구
        Input.imeCompositionMode = _PrevIme;
    }

    // 비번 입력 시작 시 IME 끄기(영문 고정)
    public void OnSelect(BaseEventData e)
    {
        _PrevIme = Input.imeCompositionMode;
        Input.imeCompositionMode = IMECompositionMode.Off;
    }
    public void OnDeselect(BaseEventData e)
    {
        Input.imeCompositionMode = _PrevIme;
    }

    void FilterAscii(string _)
    {
        if (_PWInputField == null) return;
        var src = _PWInputField.text;
        var dst = SanitizeAscii(src);
        if (dst != src)
        {
            _PWInputField.text = dst;
            _PWInputField.caretPosition = dst.Length;
            _PWInputField.ForceLabelUpdate();
        }
    }

    static string SanitizeAscii(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        s = s.Normalize(NormalizationForm.FormKC);
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
        {
            // 제로-위드/보이지 않는 문자 제거
            if (ch == '\u200B' || ch == '\u200C' || ch == '\uFEFF') continue;
            // ASCII 32~126만 허용 (공백 포함)
            if (ch >= 32 && ch <= 126) sb.Append(ch);
        }
        return sb.ToString();
    }
}
