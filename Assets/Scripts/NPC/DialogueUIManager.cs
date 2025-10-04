using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueUIManager : MonoBehaviour, IDialogueUI, IPointerClickHandler
{
    [Header("Header")]
    [SerializeField] TMP_Text _NameText;
    [SerializeField] Image _Icon;

    [Header("Body")]
    [SerializeField] TMP_Text _LineText;

    [Header("Choices (ScrollView)")]
    [SerializeField] Transform _ChoicesContent; // ScrollView/Viewport/Content
    [SerializeField] DialogueChoiceItem _ChoicePrefab;

    public bool IsOpen => gameObject.activeSelf;

    // 현재 상태가 선택지인지(= 클릭으로 넘기면 안 됨)
    bool _isChoiceMode = false;

    void OnEnable()
    {
        var dm = DialogueManager._Inst;
        dm.OnShowLine    += HandleLine;
        dm.OnShowChoices += HandleChoices;
        dm.OnEnd         += HandleEnd;

        _isChoiceMode = false;
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
        ClearChoices();
    }

    void HandleLine(string speaker, Sprite icon, string text, int idx, int total)
    {
        _isChoiceMode = false;

        if (_NameText)  _NameText.text = speaker ?? "";
        if (_LineText)     _LineText.text    = text ?? "";

        if (_Icon)
        {
            _Icon.sprite = icon;
            _Icon.color = icon ? Color.white : new Color(1,1,1,0);
        }

        ClearChoices(); // 선택지 영역 비우기
    }

    void HandleChoices(string speaker, Sprite icon, string prompt, string[] options)
    {
        _isChoiceMode = true;

        if (_NameText)  _NameText.text = speaker ?? "";
        if (_LineText)     _LineText.text    = prompt ?? "";

        if (_Icon)
        {
            _Icon.sprite = icon;
            _Icon.color = icon ? Color.white : new Color(1,1,1,0);
        }

        ClearChoices();
        for (int i = 0; i < options.Length; i++)
        {
            int idx = i;
            var item = Instantiate(_ChoicePrefab, _ChoicesContent);
            item.Bind(options[i], () => DialogueManager._Inst.Choose(idx));
        }
    }

    void HandleEnd(string npcId)
    {
        gameObject.SetActive(false); // 원하는 방식으로 닫기
        ClearChoices();
        _isChoiceMode = false;
    }

    void ClearChoices()
    {
        for (int i = _ChoicesContent.childCount - 1; i >= 0; i--)
            Destroy(_ChoicesContent.GetChild(i).gameObject);
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    // ====== 클릭으로 다음 진행 ======
    // 부모 패널에 RaycastTarget이 있는 그래픽(예: Image)이 있어야 동작함
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isChoiceMode && DialogueManager._Inst != null)
            DialogueManager._Inst.Next();
    }
}
