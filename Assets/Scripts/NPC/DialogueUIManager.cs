using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueUIManager : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject _DialogueUI;
    [SerializeField] private TMP_Text _NameText;
    [SerializeField] private Image _Icon;
    [SerializeField] private TMP_Text _DialogueText;

    void OnEnable()
    {
        DialogueManager._Inst.OnShowLine += HandleShowLine;
        DialogueManager._Inst.OnEnd += HandleEnd;
    }

    void OnDisable()
    {
        DialogueManager._Inst.OnShowLine -= HandleShowLine;
        DialogueManager._Inst.OnEnd -= HandleEnd;
    }

    void HandleShowLine(string npcName, Sprite icon, string line, int index, int total)
    {
        SetVisible(true);
        if (_NameText) _NameText.text = npcName;
        if (_DialogueText) _DialogueText.text = line;
        if (_Icon)
        {
            _Icon.sprite = icon;
            _Icon.enabled = icon != null;
        }
    }

    void HandleEnd(string npcId) => CloseNow();

    void CloseNow()
    {
        SetVisible(false);
        if (_Icon) _Icon.sprite = null;
        if (_NameText) _NameText.text = "";
        if (_DialogueText) _DialogueText.text = "";

        UIRouter._Inst?.Close<IDialogueUI>();
    }

    void SetVisible(bool on)
    {
        if (_DialogueUI) _DialogueUI.SetActive(on);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (DialogueManager._Inst?.IsRunning == true)
            DialogueManager._Inst.Next();
    }
}
