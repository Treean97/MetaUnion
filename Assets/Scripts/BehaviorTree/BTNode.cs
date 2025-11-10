public enum NodeState
{
    Running, // 계속 실행 중
    Success, // 이 노드의 목표 달성
    Failure  // 조건 불일치 / 실패
}

public abstract class BTNode
{
    // 매 프레임 Evaluate()를 호출해서 결과를 받는다.
    public abstract NodeState Evaluate();
}