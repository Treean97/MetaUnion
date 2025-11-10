using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PlayerTracker))]
public class NPCBTController : MonoBehaviour
{
    public enum NpcState
    {
        None,
        Wander,
        Watch
    }

    [Header("이동 범위")]
    [SerializeField] Transform _AreaCenter;  // null이면 시작 위치 기준
    [SerializeField] float _AreaRadius = 5f;

    [Header("Wander 설정")]
    [SerializeField] float _MinWaitTime = 3f;
    [SerializeField] float _MaxWaitTime = 5f;


    [Header("상호작용")]
    [SerializeField] float _TurnSpeedDegPerSec = 360f;
    bool _IsInteracting;
    Transform _Interactor;

    NavMeshAgent _Agent;
    PlayerTracker _Tracker;
    BTNode _Root;

    NpcState _State = NpcState.None;
    float _WaitTimer;
    Vector3 _StartPos;

    void Awake()
    {
        _Agent = GetComponent<NavMeshAgent>();
        _Tracker = GetComponent<PlayerTracker>();
        _StartPos = transform.position;

        if (_AreaCenter == null)
            _AreaCenter = transform;

        BuildTree();
    }

    void Update()
    {
        _Root?.Evaluate();
    }

    void BuildTree()
    {
        var isInteractingNode = new ConditionNode(() => _IsInteracting);
        var interactAction = new ActionNode(DoInteract);
        var interactSequence = new SequenceNode(new List<BTNode>
        {
            isInteractingNode,
            interactAction
        });

        var hasTargetNode = new ConditionNode(() => _Tracker.CurrentTarget != null);
        var watchAction = new ActionNode(DoWatch);
        var watchSequence = new SequenceNode(new List<BTNode>
        {
            hasTargetNode,
            watchAction
        });

        var noTargetNode = new ConditionNode(() => _Tracker.CurrentTarget == null && !_IsInteracting);
        var wanderAction = new ActionNode(DoWander);
        var wanderSequence = new SequenceNode(new List<BTNode>
        {
            noTargetNode,
            wanderAction
        });

        _Root = new SelectorNode(new List<BTNode>
        {
            interactSequence, // 1순위: 상호작용 중
            watchSequence,    // 2순위: 전방에 타겟
            wanderSequence    // 3순위: 그 외엔 돌아다니기
        });
    }

    // 상호작용
    NodeState DoInteract()
    {
        if (!_IsInteracting || _Interactor == null)
            return NodeState.Failure;

        _Agent.isStopped = true;
        _Agent.velocity = Vector3.zero;

        Vector3 toPlayer = _Interactor.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                _TurnSpeedDegPerSec * Time.deltaTime
            );
        }

        // 상호작용이 끝났다는 신호는 외부에서 EndInteraction()으로 줄 것.
        return NodeState.Running;
    }

    // 전방 플레이어 주시
    NodeState DoWatch()
    {
        if (_State != NpcState.Watch)
        {
            _State = NpcState.Watch;
            _Agent.isStopped = true;
            _Agent.ResetPath();
        }

        // 플레이어가 계속 시야 안에 있으면 Running 유지
        if (_Tracker.CurrentTarget != null)
        {
            // 머리 회전은 LookAtTarget이 자동으로 처리 중
            return NodeState.Running;
        }

        // 타겟이 사라지면 실패 → Selector가 Wander로 넘어간다
        return NodeState.Failure;
    }

    // 이동
    NodeState DoWander()
    {
        if (_State != NpcState.Wander)
        {
            _State = NpcState.Wander;
            _Agent.isStopped = false;
            PickNewDestination();
        }

        // 중간에 플레이어 발견하면 즉시 실패 → Watch로
        if (_Tracker.CurrentTarget != null)
            return NodeState.Failure;

        if (_Agent.pathPending)
            return NodeState.Running;

        // 목적지 도착
        if (_Agent.remainingDistance <= _Agent.stoppingDistance)
        {
            _WaitTimer -= Time.deltaTime;
            if (_WaitTimer <= 0f)
                PickNewDestination();
        }

        return NodeState.Running;
    }

    void PickNewDestination()
    {
        _WaitTimer = Random.Range(_MinWaitTime, _MaxWaitTime);

        Vector3 basePos = _AreaCenter ? _AreaCenter.position : _StartPos;

        for (int i = 0; i < 10; i++)
        {
            Vector2 rnd = Random.insideUnitCircle * _AreaRadius;
            Vector3 candidate = basePos + new Vector3(rnd.x, 0f, rnd.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                _Agent.SetDestination(hit.position);
                return;
            }
        }

        // 적당한 위치를 못 찾으면 잠시 서있기
        _Agent.ResetPath();
    }

    public void BeginInteraction(Transform player)
    {
        _IsInteracting = true;
        _Interactor = player;

        _Agent.isStopped = true;
        _Agent.ResetPath();
        _Agent.updateRotation = false; // 회전은 우리가 직접 처리
    }

    public void EndInteraction()
    {
        _IsInteracting = false;
        _Interactor = null;

        _Agent.updateRotation = true; // 다시 에이전트에 회전 위임
    }
}
