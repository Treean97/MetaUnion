using System;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager _Inst { get; private set; }

    // 항상: 이름, 아이콘, 본문(일반 텍스트), 진행도(없으면 -1)
    public event Action<string, Sprite, string, int, int> OnShowLine;
    // 선택지: 이름, 아이콘, 프롬프트(상단 텍스트), 선택지 텍스트 배열
    public event Action<string, Sprite, string, string[]> OnShowChoices;
    public event Action OnEnd;

    NPCSO _NPCSO;
    DialogueSO _DialogueSO;

    enum Mode { None, Linear, Graph }
    Mode _Mode = Mode.None;

    // Linear
    int _Idx;

    // Graph
    int _NodeId;

    public bool IsRunning => _Mode != Mode.None;

    void Awake()
    {
        if (_Inst && _Inst != this)
        {
            Destroy(this); return;
        }

        _Inst = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Play(NPCSO npc, DialogueSO overrideDialogue = null)
    {
        var dlg = overrideDialogue ? overrideDialogue : npc?.Dialogues;
        if (npc == null || dlg == null) { Debug.LogWarning("[Dialogue] 대사 없음"); return; }

        _NPCSO = npc; _DialogueSO = dlg;

        if (dlg.HasGraph)
        {
            _Mode = Mode.Graph;
            _NodeId = dlg.StartId;
            StepGraph();
        }
        else
        {
            if (dlg.Dialogues == null || dlg.Dialogues.Length == 0) { Stop(); return; }
            _Mode = Mode.Linear;
            _Idx = 0;
            EmitLinear();
        }
    }

    public void Next()
    {
        if (_Mode == Mode.Linear) NextLinear();
        else if (_Mode == Mode.Graph) NextGraph(); // Choice 상태에서는 무시
    }

    public void Choose(int index)
    {
        if (_Mode == Mode.Graph) ChooseGraph(index);
    }

    // --- Linear ---
    void EmitLinear()
    {
        string line = _DialogueSO.Dialogues[_Idx];
        OnShowLine?.Invoke(_NPCSO?.DisplayName, _NPCSO?.Icon, line, _Idx, _DialogueSO.Dialogues.Length);
    }

    void NextLinear()
    {
        _Idx++;
        if (_Idx >= _DialogueSO.Dialogues.Length) { Stop(); return; }
        EmitLinear();
    }

    // --- Graph ---
    void StepGraph()
    {
        var n = _DialogueSO.Get(_NodeId);
        if (n == null) { Stop(); return; }

        if (n is DialogueSO.LineNode ln)
        {
            OnShowLine?.Invoke( _NPCSO?.DisplayName, _NPCSO?.Icon, ln.Text, -1, -1);
        }
        else if (n is DialogueSO.ChoiceNode cn)
        {
            string[] options = new string[cn.Choices.Count];
            for (int i = 0; i < options.Length; i++) options[i] = cn.Choices[i].Text;
            OnShowChoices?.Invoke(_NPCSO?.DisplayName, _NPCSO?.Icon, cn.Text, options);
        }
    }

    void NextGraph()
    {
        var ln = _DialogueSO.Get(_NodeId) as DialogueSO.LineNode;
        if (ln == null) return;                   // Choice 상태에서는 Next 무시
        if (ln.NextId < 0) { Stop(); return; }
        _NodeId = ln.NextId; StepGraph();
    }

    void ChooseGraph(int index)
    {
        var cn = _DialogueSO.Get(_NodeId) as DialogueSO.ChoiceNode;
        if (cn == null || index < 0 || index >= cn.Choices.Count) return;

        var c = cn.Choices[index];
        if (c.NextId < 0) { Stop(); return; }
        _NodeId = c.NextId; StepGraph();
    }

    void Stop()
    {
        var id = _NPCSO?.NPCID;
        _NPCSO = null; _DialogueSO = null; _Mode = Mode.None;
        _Idx = 0; _NodeId = 0;
        OnEnd?.Invoke();
    }

    public DialogueSO.Node CurrentNode => _DialogueSO?.Get(_NodeId);
    public DialogueSO.Choice GetCurrentChoice(int index)
    {
        var cn = CurrentNode as DialogueSO.ChoiceNode;
        if (cn == null || cn.Choices == null) return null;
        if (index < 0 || index >= cn.Choices.Count) return null;
        return cn.Choices[index];
    }

    // 선택지 액션 실행 헬퍼
    public void ExecuteChoiceActions(int index)
    {
        var choice = GetCurrentChoice(index);
        var actions = choice?.Actions;
        if (actions == null) return;

        for (int i = 0; i < actions.Length; i++)
            actions[i]?.Execute();
    }

}
