// using TMPro;
// using UnityEngine;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;

// public class DialogueUIManager : MonoBehaviour, IDialogueUI, IPointerClickHandler
// {
//     [Header("Header")]
//     [SerializeField] TMP_Text _NameText;
//     [SerializeField] Image _Icon;

//     [Header("Body")]
//     [SerializeField] TMP_Text _LineText;

//     [Header("Choices (ScrollView)")]
//     [SerializeField] Transform _ChoicesContent; // ScrollView/Viewport/Content
//     [SerializeField] DialogueChoiceItem _ChoicePrefab;

//     public bool IsOpen => gameObject.activeSelf;

//     // 현재 상태가 선택지인지(= 클릭으로 넘기면 안 됨)
//     bool _IsChoiceMode = false;

//     void OnEnable()
//     {
//         var dm = DialogueManager._Inst;
//         dm.OnShowLine    += HandleLine;
//         dm.OnShowChoices += HandleChoices;
//         dm.OnEnd         += HandleEnd;

//         _IsChoiceMode = false;
//         ClearChoices();
//     }

//     void OnDisable()
//     {
//         var dm = DialogueManager._Inst;
//         if (dm != null)
//         {
//             dm.OnShowLine    -= HandleLine;
//             dm.OnShowChoices -= HandleChoices;
//             dm.OnEnd         -= HandleEnd;
//         }
//         ClearChoices();
//     }

//     void HandleLine(string speaker, Sprite icon, string text, int idx, int total)
//     {
//         _IsChoiceMode = false;

//         if (_NameText)  _NameText.text = speaker ?? "";
//         if (_LineText)     _LineText.text    = text ?? "";

//         if (_Icon)
//         {
//             _Icon.sprite = icon;
//             _Icon.color = icon ? Color.white : new Color(1,1,1,0);
//         }
        
//         ClearChoices(); // 선택지 영역 비우기
//     }

//     void HandleChoices(string speaker, Sprite icon, string prompt, string[] options)
//     {
//         _IsChoiceMode = true;

//         if (_NameText)  _NameText.text = speaker ?? "";
//         if (_LineText)     _LineText.text    = prompt ?? "";

//         if (_Icon)
//         {
//             _Icon.sprite = icon;
//             _Icon.color = icon ? Color.white : new Color(1,1,1,0);
//         }

//         ClearChoices();
//         for (int i = 0; i < options.Length; i++)
//         {
//             int idx = i;
//             var item = Instantiate(_ChoicePrefab, _ChoicesContent);
//             item.Bind(options[i], () => DialogueManager._Inst.Choose(idx));
//         }
//     }

//     void HandleEnd(string npcId)
//     {
//         gameObject.SetActive(false); // 원하는 방식으로 닫기
//         ClearChoices();
//         _IsChoiceMode = false;
//     }

//     void ClearChoices()
//     {
//         for (int i = _ChoicesContent.childCount - 1; i >= 0; i--)
//             Destroy(_ChoicesContent.GetChild(i).gameObject);
//     }

//     public void Show() => gameObject.SetActive(true);
//     public void Hide() => gameObject.SetActive(false);

//     // ====== 클릭으로 다음 진행 ======
//     // 부모 패널에 RaycastTarget이 있는 그래픽(예: Image)이 있어야 동작함
//     public void OnPointerClick(PointerEventData eventData)
//     {
//         if (!_IsChoiceMode && DialogueManager._Inst != null)
//             DialogueManager._Inst.Next();
//     }
// }
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;

public class DialogueUIManager : MonoBehaviour, IDialogueUI, IPointerClickHandler
{
    [Header("Header")]
    [SerializeField] TMP_Text _NameText;
    [SerializeField] Image _Icon;

    [Header("Body")]
    [SerializeField] TMP_Text _LineText;

    [Header("Choices (ScrollView)")]
    [SerializeField] Transform _ChoicesContent;
    [SerializeField] DialogueChoiceItem _ChoicePrefab;

    [Header("Typewriter (Lines Only)")]
    [SerializeField] float _CharsPerSecond = 30f;

    public bool IsOpen => gameObject.activeSelf;

    bool _IsChoiceMode = false;
    Coroutine _TypeCo;
    bool _IsTyping;
    int _TargetVisible;
    Action _OnTypeEnd; // 타자 종료 콜백

    void OnEnable()
    {
        var dm = DialogueManager._Inst;
        dm.OnShowLine    += HandleLine;
        dm.OnShowChoices += HandleChoices;
        dm.OnEnd         += HandleEnd;

        _IsChoiceMode = false;
        ClearChoices();
    }

    void OnDisable()
    {
        var dm = DialogueManager._Inst;
        if (dm != null)
        {
            dm.OnShowLine    -= HandleLine;
            dm.OnShowChoices -= HandleChoices;
            dm.OnEnd         -= HandleEnd;
        }
        StopTypewriter();
        ClearChoices();
    }

    // ===== 일반 대사(타자효과) =====
    void HandleLine(string speaker, Sprite icon, string text, int idx, int total)
    {
        _IsChoiceMode = false;

        if (_NameText) _NameText.text = speaker ?? "";
        if (_Icon) { _Icon.sprite = icon; _Icon.color = icon ? Color.white : new Color(1,1,1,0); }

        ClearChoices();

        if (!_LineText) return;
        StartTypewriter(text ?? "", null); // 대사는 그냥 타자만
    }

    // ===== 선택지(프롬프트 타자효과 + 종료 후 옵션 생성) =====
    void HandleChoices(string speaker, Sprite icon, string prompt, string[] options)
    {
        _IsChoiceMode = true;

        if (_NameText) _NameText.text = speaker ?? "";
        if (_Icon) { _Icon.sprite = icon; _Icon.color = icon ? Color.white : new Color(1,1,1,0); }

        ClearChoices();

        if (_LineText)
        {
            // 프롬프트를 타자효과로 출력하고, 끝나면 선택지 생성
            StartTypewriter(prompt ?? "", () =>
            {
                BuildChoices(options);
            });
        }
        else
        {
            // 혹시 _LineText가 없으면 바로 생성
            BuildChoices(options);
        }
    }

    void BuildChoices(string[] options)
    {
        for (int i = 0; i < options.Length; i++)
        {
            int idx = i;
            var item = Instantiate(_ChoicePrefab, _ChoicesContent);
            item.Bind(options[i], () => DialogueManager._Inst.Choose(idx));
        }
    }

    void HandleEnd(string npcId)
    {
        StopTypewriter();
        gameObject.SetActive(false);
        ClearChoices();
        _IsChoiceMode = false;
    }

    void ClearChoices()
    {
        for (int i = _ChoicesContent.childCount - 1; i >= 0; i--)
            Destroy(_ChoicesContent.GetChild(i).gameObject);
    }

    public void Show() { }
    public void Hide() { StopTypewriter(); }

    // ===== 타자효과 =====
    void StartTypewriter(string fullText, Action onDone)
    {
        StopTypewriter();              // 진행 중이면 정리 + onDone 호출 처리 포함
        if (!_LineText) return;

        _OnTypeEnd = onDone;          // 종료 콜백 저장

        _LineText.text = fullText;
        _LineText.maxVisibleCharacters = 0;
        _LineText.ForceMeshUpdate();
        _TargetVisible = _LineText.textInfo.characterCount;

        if (_CharsPerSecond <= 0f || _TargetVisible <= 0)
        {
            _LineText.maxVisibleCharacters = int.MaxValue;
            var cb = _OnTypeEnd; _OnTypeEnd = null;
            cb?.Invoke();
            _IsTyping = false;
            return;
        }

        _TypeCo = StartCoroutine(TypeLineCo());
    }

    void StopTypewriter()
    {
        if (_TypeCo != null)
        {
            StopCoroutine(_TypeCo);
            _TypeCo = null;
        }

        if (_LineText) _LineText.maxVisibleCharacters = int.MaxValue;

        // 스킵 시에도 onDone은 즉시 호출되어야 선택지가 바로 뜸
        if (_OnTypeEnd != null)
        {
            var cb = _OnTypeEnd; _OnTypeEnd = null;
            cb.Invoke();
        }

        _IsTyping = false;
    }

    IEnumerator TypeLineCo()
    {
        _IsTyping = true;
        float step = 1f / Mathf.Max(1e-6f, _CharsPerSecond); // 0 방지
        int visible = 0;

        while (visible < _TargetVisible)
        {
            visible++;
            _LineText.maxVisibleCharacters = visible;
            yield return new WaitForSecondsRealtime(step);
        }

        _IsTyping = false;
        _TypeCo = null;
        _LineText.maxVisibleCharacters = int.MaxValue;

        // 정상 종료 시 콜백 실행
        if (_OnTypeEnd != null)
        {
            var cb = _OnTypeEnd; _OnTypeEnd = null;
            cb.Invoke();
        }
    }

    // 클릭 진행/스킵
    public void OnPointerClick(PointerEventData eventData)
    {
        var dm = DialogueManager._Inst;
        if (dm == null) return;

        if (_IsChoiceMode)
        {
            // 선택지 상태에서 바디 클릭은 무시 (버튼으로만 진행)
            return;
        }

        if (_IsTyping)
        {
            // 타자효과 스킵 → 즉시 완성 + onDone 실행
            StopTypewriter();
            return;
        }

        dm.Next(); // 다음 대사
    }
}
