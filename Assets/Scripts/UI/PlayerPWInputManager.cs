using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_InputField))]
public class PlayerPWInputManager : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    TMP_InputField _InputField;

    IMECompositionMode _PrevIme;

    void Awake()
    {
        _InputField = GetComponent<TMP_InputField>();

        // === Password 모드 고정 ===
        _InputField.contentType = TMP_InputField.ContentType.Password; // TMP가 자동으로 '*' 마스킹
        _InputField.asteriskChar = '*';
        _InputField.lineType     = TMP_InputField.LineType.SingleLine;
        _InputField.richText     = false;

        // 모바일 키보드: ASCII 전용 레이아웃 권장
        _InputField.keyboardType = TouchScreenKeyboardType.ASCIICapable;

        // 1) 문자 단위 유효성 검사 (입력 시점에서 걸러냄)
        _InputField.onValidateInput += ValidateAsciiOnly;

        // 2) 안전망: 붙여넣기 등으로 들어온 비ASCII를 한 번 더 정리
        _InputField.onValueChanged.AddListener(SanitizeAsciiOnly);
    }

    void OnDestroy()
    {
        if (_InputField)
        {
            _InputField.onValidateInput -= ValidateAsciiOnly;
            _InputField.onValueChanged.RemoveListener(SanitizeAsciiOnly);
        }
    }

    // === IME 제어: 선택 시 한글 IME 끄기, 해제 시 원복 ===
    public void OnSelect(BaseEventData e)
    {
        _PrevIme = Input.imeCompositionMode;
        Input.imeCompositionMode = IMECompositionMode.Off;
    }

    public void OnDeselect(BaseEventData e)
    {
        Input.imeCompositionMode = _PrevIme;
    }

    // 실제 비밀번호 읽기
    public string GetPassword() => _InputField ? _InputField.text : string.Empty;

    public void ClearPassword()
    {
        if (!_InputField) return;
        _InputField.SetTextWithoutNotify(string.Empty);
        _InputField.caretPosition = 0;
    }

    // ========= 헬퍼 =========

    // 단일 문자 유효성 검사: 0x20~0x7E(표시 가능한 ASCII)만 허용
    char ValidateAsciiOnly(string text, int charIndex, char added)
    {
        return (added >= 0x20 && added <= 0x7E) ? added : '\0';
    }

    // 전체 문자열 안전망: 비ASCII 전부 제거 + 캐럿 보정
    void SanitizeAsciiOnly(string s)
    {
        if (_InputField == null) return;
        if (string.IsNullOrEmpty(s)) return;

        // 빠른 경로: 전부 ASCII면 그대로
        bool allAscii = true;
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (c < 0x20 || c > 0x7E) { allAscii = false; break; }
        }
        if (allAscii) return;

        // 정화
        System.Text.StringBuilder sb = new System.Text.StringBuilder(s.Length);
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (c >= 0x20 && c <= 0x7E) sb.Append(c);
        }

        int caret = Mathf.Min(_InputField.caretPosition, sb.Length);
        _InputField.SetTextWithoutNotify(sb.ToString());
        _InputField.caretPosition = caret;
    }
}
