using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class PlayerIDInputCondition : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] TMP_InputField _Input;

    [Header("글자 수 제한")]
    [SerializeField] int _MinLength = 4;
    [SerializeField] int _MaxLength = 20;

    // 현재 입력이 유효한지 여부(필요하면 버튼 제어 등에 사용)
    public bool IsValid { get; private set; }

    // 한글(가~힣) + 영문만 허용
    static readonly Regex _Regex = new Regex("^[a-zA-Z\uAC00-\uD7A3]+$");

    void Awake()
    {
        if (!_Input)
            _Input = GetComponent<TMP_InputField>();
    }

    void OnEnable()
    {
        if (_Input)
        {
            _Input.onValueChanged.AddListener(Validate);
            Validate(_Input.text);   // 초기 값도 한 번 검사
        }
        else
        {
            Validate(string.Empty);
        }
    }

    void OnDisable()
    {
        if (_Input)
            _Input.onValueChanged.RemoveListener(Validate);
    }

    void Validate(string value)
    {
        value ??= "";
        var trimmed = value.Trim();

        IsValid = true;

        // 비어 있으면 무조건 불가
        if (string.IsNullOrEmpty(trimmed))
        {
            IsValid = false;
            return;
        }

        // 길이 제한
        if (trimmed.Length < _MinLength)
        {
            IsValid = false;
        }
        else if (trimmed.Length > _MaxLength)
        {
            IsValid = false;
        }

        // 한글 + 영문만 허용
        if (IsValid && !_Regex.IsMatch(trimmed))
        {
            IsValid = false;
        }
    }

}
