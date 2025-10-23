using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "DialogueData")]
public class DialogueSO : ScriptableObject
{
    [Header("Linear (선형)")]
    [SerializeField] private string[] _Dialogues;
    public string[] Dialogues => _Dialogues;

    [Header("Graph (선택지)")]
    [SerializeField] private int _StartId = 0;

    [SerializeReference] private List<Node> _nodes = new();

    // 실제 노드가 하나라도 있을 때만 true (null 슬롯 제외)
    public bool HasGraph => _nodes != null && _nodes.Exists(n => n != null);
    public int StartId => _StartId;

    public Node Get(int id)
    {
        if (_nodes == null) return null;
        return _nodes.Find(n => n != null && n.Id == id);
    }

    [Serializable] public abstract class Node
    {
        public int Id;
        [TextArea] public string Text;
    }

    [Serializable] public class LineNode   : Node { public int NextId = -1; } // -1: 종료
    [Serializable] public class ChoiceNode : Node { public List<Choice> Choices = new(); }

    [Serializable] public class Choice
    {
        public string Text;
        public int NextId = -1; // -1: 종료
        public ChoiceActionSO[] Actions;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (_nodes == null) return;

        // null 슬롯은 건드리지 않고(사용자가 타입 지정할 수 있게) 검사만 안전하게
        var ids = new HashSet<int>();
        foreach (var n in _nodes)
        {
            if (n == null) continue;           // null 가드
            if (!ids.Add(n.Id))
                Debug.LogWarning($"{name}: 중복 Node Id {n.Id}", this);
        }

        // 실제 노드가 있을 때만 StartId 유효성 체크
        if (HasGraph && Get(_StartId) == null)
            Debug.LogWarning($"{name}: StartId({_StartId}) 노드가 없습니다.", this);
    }

    [ContextMenu("Add Line Node")]
    void CtxAddLine()
    {
        if (_nodes == null) _nodes = new List<Node>();
        int id = GetNextId();
        _nodes.Add(new LineNode { Id = id, Text = "Line", NextId = -1 });
        if (Get(_StartId) == null) _StartId = id; // 첫 노드면 시작으로
        EditorUtility.SetDirty(this);
    }

    [ContextMenu("Add Choice Node")]
    void CtxAddChoice()
    {
        if (_nodes == null) _nodes = new List<Node>();
        int id = GetNextId();
        _nodes.Add(new ChoiceNode { Id = id, Text = "Choose...", Choices = new List<Choice>() });
        if (Get(_StartId) == null) _StartId = id; // 첫 노드면 시작으로
        EditorUtility.SetDirty(this);
    }

    [ContextMenu("Clear Null Nodes")]
    void CtxClearNull()
    {
        _nodes?.RemoveAll(n => n == null);
        EditorUtility.SetDirty(this);
    }

    int GetNextId()
    {
        int max = -1;
        if (_nodes != null)
            foreach (var n in _nodes)
                if (n != null) max = Mathf.Max(max, n.Id);
        return max + 1;
    }
#endif
}
