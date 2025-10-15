using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    [SerializeField] NPCSO _NPCSO;
    public NPCSO NPCSO => _NPCSO;
    private DialogueSO _DialogueSO;    

    InfoDataSO _TempFocusInfo;

    void Awake()
    {
        _DialogueSO = _NPCSO.Dialogues;
    }

    public InfoDataSO GetObjectInfo()
    {
        // 기존 인터페이스를 반드시 지켜야 한다면 임시 SO 반환
        if (_TempFocusInfo == null)
        {
            _TempFocusInfo = ScriptableObject.CreateInstance<InfoDataSO>();
            _TempFocusInfo.DisplayName = _NPCSO.DisplayName;
            _TempFocusInfo.Description = "";
        }
        return _TempFocusInfo;
    }
   

    public void OnDefocus()
    {
        GameEvents.RaiseDefocus();
    }

    public void OnFocus()
    {
        var info = GetObjectInfo();
        GameEvents.RaiseFocus(info);
    }

    public void OnInteract()
    {
        if (DialogueManager._Inst == null || _NPCSO == null)
        {
            Debug.LogWarning("[NPC] DialogueService 또는 NPCSO 누락");
            return;
        }

        // 이미 대화 중이면 중복 실행 방지
        if (DialogueManager._Inst.IsRunning) return;

        UIRouter._Inst?.Open<IDialogueUI>();
        DialogueManager._Inst.Play(_NPCSO, _DialogueSO);
        // 대화 애니메이션 트리거
        GetComponent<Animator>().SetTrigger("TalkTrigger");
        OnDefocus();
    }

}
