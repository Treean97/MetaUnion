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
    Action OnTypeEnd; // 타자 종료 콜백
    public static event Action OnClick; // 컷신용 이벤트

    void OnEnable()
    {
        var dm = DialogueManager._Inst;
        dm.OnShowLine += HandleLine;
        dm.OnShowChoices += HandleChoices;
        dm.OnEnd += HandleEnd;

        _IsChoiceMode = false;
        ClearChoices();
    }

    void OnDisable()
    {
        var dm = DialogueManager._Inst;
        if (dm != null)
        {
            dm.OnShowLine -= HandleLine;
            dm.OnShowChoices -= HandleChoices;
            dm.OnEnd -= HandleEnd;
        }
        StopTypewriter();
        ClearChoices();
    }

    // 일반 대사
    void HandleLine(string speaker, Sprite icon, string text, int idx, int total)
    {
        _IsChoiceMode = false;

        if (_NameText) _NameText.text = speaker ?? "";
        if (_Icon) { _Icon.sprite = icon; _Icon.color = icon ? Color.white : new Color(1,1,1,0); }

        ClearChoices();

        if (!_LineText) return;
        StartTypewriter(text ?? "", null); // 대사는 그냥 타자만
    }

    // 선택지
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
            item.Bind(options[i], () => {
                // 선택지 액션
                DialogueManager._Inst.ExecuteChoiceActions(idx);
                // 대화 분기
                DialogueManager._Inst.Choose(idx);
                // 외부 알림
                OnClick?.Invoke();
                
                });
        }        
    }

    void HandleEnd()
    {
        StopTypewriter();
        UIFX.Hide(gameObject);
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

        OnTypeEnd = onDone;          // 종료 콜백 저장

        _LineText.text = fullText;
        _LineText.maxVisibleCharacters = 0;
        _LineText.ForceMeshUpdate();
        _TargetVisible = _LineText.textInfo.characterCount;

        if (_CharsPerSecond <= 0f || _TargetVisible <= 0)
        {
            _LineText.maxVisibleCharacters = int.MaxValue;
            var cb = OnTypeEnd; OnTypeEnd = null;
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
        if (OnTypeEnd != null)
        {
            var cb = OnTypeEnd; OnTypeEnd = null;
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
        if (OnTypeEnd != null)
        {
            var cb = OnTypeEnd; OnTypeEnd = null;
            cb.Invoke();
        }
    }

    // 클릭 진행/스킵
    public void OnPointerClick(PointerEventData eventData)
    {
        var dm = DialogueManager._Inst;
        if (dm == null) return;

        // 선택지 모드일 땐 바디 클릭 무시
        if (_IsChoiceMode)
            return;

        // 타자 효과 진행 중이면 스킵
        if (_IsTyping)
        {
            StopTypewriter();
            return;
        }

        var tm = TimelineManager._Inst;

        // 타임라인이 없거나, 안 돌고 있으면 → 그냥 대사만 진행
        if (tm == null || !tm.IsRunning)
        {
            dm.Next();
            return;
        }

        // 타임라인이 대사를 막는 중이면 아무것도 안 함
        if (tm.IsBlockDialogue)
            return;

        // 여기까지 왔으면 타임라인/대사 둘 다 모드에 따라 처리
        CheckTimeline(tm);
    }


    void CheckTimeline(TimelineManager tm)
    {
        switch(tm.Mode)
        {
            case TimelineManager.ClickMode.DialogueAndTimeline:
                OnClick?.Invoke(); // 컷신 진행 이벤트
                DialogueManager._Inst.Next(); // 대사 진행
                break;

            case TimelineManager.ClickMode.Timeline:
                OnClick?.Invoke(); // 컷신 진행 이벤트
                break;

            case TimelineManager.ClickMode.Dialogue:
                DialogueManager._Inst.Next();
                break;
        }
    }
}
