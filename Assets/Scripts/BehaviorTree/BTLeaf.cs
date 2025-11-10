using System;

public class ConditionNode : BTNode
{
    private readonly Func<bool> _Predicate;

    public ConditionNode(Func<bool> predicate)
    {
        _Predicate = predicate;
    }

    public override NodeState Evaluate()
    {
        return _Predicate() ? NodeState.Success : NodeState.Failure;
    }
}

public class ActionNode : BTNode
{
    private readonly Func<NodeState> _action;

    public ActionNode(Func<NodeState> action)
    {
        _action = action;
    }

    public override NodeState Evaluate()
    {
        return _action();
    }
}
