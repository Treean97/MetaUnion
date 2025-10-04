using System;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager _Inst { get; private set; }

    public event Action<string, Sprite, string, int, int> OnShowLine;
    public event Action<string> OnEnd;

    NPCSO _NPC;
    DialogueSO _Dialogue;
    int _Idx;
    public bool IsRunning => _Dialogue != null;

    void Awake()
    {
        if (_Inst && _Inst != this)
        {
            Destroy(this);
            return;
        }   

        _Inst = this; DontDestroyOnLoad(gameObject);
    }

    public void Play(NPCSO npc, DialogueSO overrideDialogue = null)
    {
        var dlg = overrideDialogue ? overrideDialogue : npc?.Dialogues;
        if (npc == null || dlg == null || dlg.Dialogues == null || dlg.Dialogues.Length == 0) { Debug.LogWarning("[Dialogue] 대사 없음"); return; }
        _NPC = npc; _Dialogue = dlg; _Idx = 0; Emit();
    }

    public void Next()
    {
        if (_Dialogue == null) return;
        _Idx++;
        if (_Idx >= _Dialogue.Dialogues.Length) { var id = _NPC?.NPCID; _NPC=null; _Dialogue=null; _Idx=0; OnEnd?.Invoke(id); return; }
        Emit();
    }

    void Emit()
    {
        string line = _Dialogue.Dialogues[_Idx];
        OnShowLine?.Invoke(_NPC?.DisplayName ?? "NPC", _NPC?.Icon, line, _Idx, _Dialogue.Dialogues.Length);
    }
}
