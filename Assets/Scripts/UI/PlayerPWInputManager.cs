using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_InputField))]
public class PlayerPWInputManager : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    TMP_InputField _InputField;
    readonly StringBuilder _StringBuilder = new(); // 비밀번호 버퍼

    IMECompositionMode _PrevIme;

    void Awake()
    {
        _InputField = GetComponent<TMP_InputField>();

        _InputField.contentType = TMP_InputField.ContentType.Standard; // Password 사용 안함
        _InputField.lineType    = TMP_InputField.LineType.SingleLine;
        _InputField.richText    = false;

        _InputField.onValidateInput += ValidateAndMask;
        _InputField.onValueChanged.AddListener(SyncBufferByMaskedLength);
    }

    void OnDestroy()
    {
        if (_InputField)
        {
            _InputField.onValidateInput -= ValidateAndMask;
            _InputField.onValueChanged.RemoveListener(SyncBufferByMaskedLength);
        }
    }

    void Update()
    {
        // IME 조합 문자열이 생기면 TMP가 조합 미리보기를 붙임 -> 즉시 별표로 되돌려 억제
        if (!string.IsNullOrEmpty(Input.compositionString))
        {
            var stars = new string('*', _StringBuilder.Length);
            if (_InputField.text != stars)
            {
                _InputField.SetTextWithoutNotify(stars);
                _InputField.caretPosition = _StringBuilder.Length;
            }
        }
    }

    // IME 제어
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
    public string GetPassword() => _StringBuilder.ToString();

    // 입력 검증: ASCII만 허용, 화면엔 '*'만
    char ValidateAndMask(string text, int charIndex, char added)
    {
        if (added < 32 || added > 126) return '\0'; // 비ASCII/제어문자 차단

        if (charIndex < 0 || charIndex > _StringBuilder.Length) charIndex = _StringBuilder.Length;
        _StringBuilder.Insert(charIndex, added);
        return '*';
    }

    // 삭제/붙여넣기 동기화
    void SyncBufferByMaskedLength(string masked)
    {
        int mLen = masked?.Length ?? 0;

        if (mLen < _StringBuilder.Length)
        {
            _StringBuilder.Length = mLen; // 삭제 반영
        }
        else if (mLen > _StringBuilder.Length)
        {
            // onValidate에서 못 걸렀을 때 안전망
            var stars = new string('*', _StringBuilder.Length);
            _InputField.SetTextWithoutNotify(stars);
            _InputField.caretPosition = _StringBuilder.Length;
        }
    }

    public void ClearPassword()
    {
        _StringBuilder.Clear();
        _InputField.SetTextWithoutNotify(string.Empty);
        _InputField.caretPosition = 0;
    }
}
