using UnityEngine;

public class NPC : MonoBehaviour, IInteractable, IInteractionReceiver
{
    [SerializeField] NPCSO _NPCSO;
    public NPCSO NPCSO => _NPCSO;
    private DialogueSO _DialogueSO;    

    InfoDataSO _TempFocusInfo;

    NPCBTController _Ai;

    void Awake()
    {
        _Ai = GetComponent<NPCBTController>();
        _DialogueSO = _NPCSO.Dialogues;
    }

    public InfoDataSO GetObjectInfo()
    {
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

    // === IInteractable ===
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

        GetComponent<Animator>().SetTrigger("TalkTrigger");
        OnDefocus();
    }

    // === IInteractionReceiver ===
    public void BeginInteraction(Transform interactor)
    {
        // 행동트리에 대화 시작 알림
        _Ai?.BeginInteraction(interactor);
    }

    public void EndInteraction()
    {
        _Ai?.EndInteraction();
    }
}
