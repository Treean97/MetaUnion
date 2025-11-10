using System.Collections.Generic;

public class SelectorNode : BTNode
{
    private readonly List<BTNode> _Children;

    public SelectorNode(List<BTNode> children)
    {
        _Children = children;
    }

    public override NodeState Evaluate()
    {
        foreach (var child in _Children)
        {
            var state = child.Evaluate();

            // 하나라도 Success/Running이면 거기서 멈추고 그 결과 반환
            if (state == NodeState.Success || state == NodeState.Running)
                return state;
        }
        // 전부 Failure면 Failure
        return NodeState.Failure;
    }
}

public class SequenceNode : BTNode
{
    private readonly List<BTNode> _Children;

    public SequenceNode(List<BTNode> children)
    {
        _Children = children;
    }

    public override NodeState Evaluate()
    {
        bool anyRunning = false;

        foreach (var child in _Children)
        {
            var state = child.Evaluate();

            if (state == NodeState.Failure)
                return NodeState.Failure;

            if (state == NodeState.Running)
                anyRunning = true;
        }

        if (anyRunning)
            return NodeState.Running;

        return NodeState.Success;
    }
}
