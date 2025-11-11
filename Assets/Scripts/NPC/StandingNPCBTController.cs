using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerTracker))]
public class StandingNPCBTController : MonoBehaviour, INPCBrain, INPCAnimSource
{
    public enum NpcState
    {
        None,
        Idle,
        Watch
    }

    [Header("회전 속도")]
    [SerializeField] float _TurnSpeedDegPerSec = 360f;

    bool _IsInteracting;
    Transform _Interactor;

    PlayerTracker _Tracker;
    BTNode _Root;

    NpcState _State = NpcState.None;
    public NpcState State => _State;

    // === INPCAnimSource ===
    public NPCAnimState CurrentAnimState { get; private set; } = NPCAnimState.Idle;
    public event Action<NPCAnimState> OnAnimStateChanged;

    // 필요하면 여전히 직접 상태 구독도 가능
    public event Action<NpcState> OnStateChanged;

    void Awake()
    {
        _Tracker = GetComponent<PlayerTracker>();
        BuildTree();

        // 초기 애니 상태 한번 동기화
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
            // 고정형은 Idle/Watch 둘 다 Idle 애니로 처리
            switch (_State)
            {
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

        var hasTargetNode = new ConditionNode(() => _Tracker && _Tracker.CurrentTarget != null);
        var watchAction   = new ActionNode(DoWatch);
        var watchSequence = new SequenceNode(new List<BTNode>
        {
            hasTargetNode,
            watchAction
        });

        var idleAction   = new ActionNode(DoIdle);

        _Root = new SelectorNode(new List<BTNode>
        {
            interactSequence, // 1순위: 대화 중
            watchSequence,    // 2순위: 전방에 플레이어
            idleAction        // 3순위: 아무도 없으면 Idle
        });
    }

    // 상호작용 상태: 대화 중인 플레이어 방향으로 몸 회전
    NodeState DoInteract()
    {
        if (!_IsInteracting || _Interactor == null)
            return NodeState.Failure;

        SetState(NpcState.Watch); // 대화 중에는 Watch 계열

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
        if (_Tracker == null || _Tracker.CurrentTarget == null)
            return NodeState.Failure;

        SetState(NpcState.Watch);

        Vector3 toTarget = _Tracker.CurrentTarget.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                _TurnSpeedDegPerSec * Time.deltaTime
            );
        }

        return NodeState.Running;
    }

    // Idle 상태: 그냥 제자리
    NodeState DoIdle()
    {
        if (_State != NpcState.Idle)
            SetState(NpcState.Idle);

        return NodeState.Running;
    }

    // === INPCBrain ===
    public void BeginInteraction(Transform interactor)
    {
        _IsInteracting = true;
        _Interactor    = interactor;
        UpdateAnimState();
    }

    public void EndInteraction()
    {
        _IsInteracting = false;
        _Interactor    = null;
        UpdateAnimState();
    }
}
