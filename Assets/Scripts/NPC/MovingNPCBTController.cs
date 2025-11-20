using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PlayerTracker))]
public class MovingNPCBTController : MonoBehaviour, INPCBrain, INPCAnimSource
{
    public enum NpcState
    {
        None,
        Idle,
        Wander,
        Watch
    }

    [Header("네비메시 / 이동")]
    [SerializeField] string _NavAreaName   = "NPCWalkArea";
    [SerializeField] float  _MaxTravelTime = 10f;
    [SerializeField] float  _RoamRadius    = 10f;

    [Header("정지(대기 시간)")]
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
    float _TravelTimer;

    int _RoamAreaMask = NavMesh.AllAreas;

    public NpcState State => _State;
    public event Action<NpcState> OnStateChanged;

    // === INPCAnimSource ===
    public NPCAnimState CurrentAnimState { get; private set; } = NPCAnimState.Idle;
    public event Action<NPCAnimState> OnAnimStateChanged;

    void Awake()
    {
        _Agent   = GetComponent<NavMeshAgent>();
        _Tracker = GetComponent<PlayerTracker>();

        int areaIndex = NavMesh.GetAreaFromName(_NavAreaName);
        if (areaIndex < 0)
        {
            Debug.LogWarning($"[MovingNPCBTController] NavMesh Area '{_NavAreaName}' 를 찾지 못했습니다. 전체 영역 사용");
            _RoamAreaMask = NavMesh.AllAreas;
        }
        else
        {
            _RoamAreaMask   = 1 << areaIndex;
            _Agent.areaMask = _RoamAreaMask;
        }

        BuildTree();
        UpdateAnimState();
    }

    void Update()
    {
        _Root?.Evaluate();
    }

    void SetState(NpcState newState)
    {
        if (_State == newState) return;
        _State = newState;
        OnStateChanged?.Invoke(_State);
        UpdateAnimState();
    }

    void UpdateAnimState()
    {
        NPCAnimState next;

        if (_IsInteracting)
        {
            next = NPCAnimState.Interact;
        }
        else
        {
            switch (_State)
            {
                case NpcState.Wander:
                    next = NPCAnimState.Walk;
                    break;

                case NpcState.Idle:
                case NpcState.Watch:
                case NpcState.None:
                default:
                    next = NPCAnimState.Idle;
                    break;
            }
        }

        if (next == CurrentAnimState) return;

        CurrentAnimState = next;
        OnAnimStateChanged?.Invoke(CurrentAnimState);
    }

    void BuildTree()
    {
        var isInteractingNode = new ConditionNode(() => _IsInteracting);
        var interactAction    = new ActionNode(DoInteract);
        var interactSequence  = new SequenceNode(new List<BTNode>
        {
            isInteractingNode,
            interactAction
        });

        var hasTargetNode = new ConditionNode(() => _Tracker.CurrentTarget != null);
        var watchAction   = new ActionNode(DoWatch);
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
            interactSequence,
            watchSequence,
            wanderSequence
        });
    }

    // 상호작용 상태
    NodeState DoInteract()
    {
        if (!_IsInteracting || _Interactor == null)
            return NodeState.Failure;

        _Agent.isStopped = true;
        _Agent.velocity  = Vector3.zero;

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

        return NodeState.Running;
    }

    // 전방 플레이어 주시
    NodeState DoWatch()
    {
        if (_State != NpcState.Watch)
        {
            SetState(NpcState.Watch);
            _Agent.isStopped = true;
        }

        if (_Tracker.CurrentTarget != null)
            return NodeState.Running;

        return NodeState.Failure;
    }

    // 배회(Wander + Idle)
    NodeState DoWander()
    {
        if (_State != NpcState.Wander && _State != NpcState.Idle)
        {
            SetState(NpcState.Wander);
            _Agent.isStopped = false;

            if (!_Agent.hasPath || _Agent.remainingDistance <= _Agent.stoppingDistance)
            {
                PickNewDestination();
            }
        }

        if (_Tracker.CurrentTarget != null)
            return NodeState.Failure;

        if (_Agent.pathPending)
            return NodeState.Running;

        // 이동 중
        if (_Agent.hasPath && _Agent.remainingDistance > _Agent.stoppingDistance)
        {
            SetState(NpcState.Wander);

            _TravelTimer += Time.deltaTime;

            if (_Agent.pathStatus == NavMeshPathStatus.PathInvalid ||
                _TravelTimer > _MaxTravelTime)
            {
                _Agent.ResetPath();
                PickNewDestination();
                return NodeState.Running;
            }

            return NodeState.Running;
        }

        // 목적지 도착 → Idle 상태로 전환 후 대기
        if (_Agent.remainingDistance <= _Agent.stoppingDistance)
        {
            // 멈추고 경로 초기화
            if (!_Agent.isStopped)
            {
                _Agent.isStopped = true;
                _Agent.velocity  = Vector3.zero;
                _Agent.ResetPath();
            }

            SetState(NpcState.Idle);

            _WaitTimer -= Time.deltaTime;
            if (_WaitTimer <= 0f)
            {
                // 다음 목적지로 다시 출발
                PickNewDestination();
                SetState(NpcState.Wander);
            }
        }

        return NodeState.Running;

    }

    void PickNewDestination()
    {
        _WaitTimer   = Random.Range(_MinWaitTime, _MaxWaitTime);
        _TravelTimer = 0f;

        Vector3 basePos = transform.position;

        for (int i = 0; i < 10; i++)
        {
            Vector2 rnd = Random.insideUnitCircle * _RoamRadius;
            Vector3 candidate = basePos + new Vector3(rnd.x, 0f, rnd.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, _RoamAreaMask))
            {
                _Agent.isStopped = false;
                _Agent.SetDestination(hit.position);
                return;
            }
        }

        _Agent.ResetPath();
        SetState(NpcState.Idle);
    }

    // === INPCBrain ===
    public void BeginInteraction(Transform player)
    {
        _IsInteracting = true;
        _Interactor    = player;

        _Agent.isStopped      = true;
        _Agent.updateRotation = false;

        UpdateAnimState();
    }

    public void EndInteraction()
    {
        _IsInteracting = false;
        _Interactor    = null;

        _Agent.updateRotation = true;

        UpdateAnimState();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.4f);

        Vector3 center = transform.position;

        const int segments = 32;
        float angleStep = Mathf.PI * 2f / segments;
        Vector3 prev = center + new Vector3(_RoamRadius, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float a = i * angleStep;
            Vector3 next = center + new Vector3(Mathf.Cos(a) * _RoamRadius, 0f, Mathf.Sin(a) * _RoamRadius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
#endif
}
