using UnityEngine;

// 1) 상태 인터페이스
public interface IPlayerState
{
    void EnterState();
    void UpdateState();
    void ExitState();
}

// 2) 상태 머신 컴포넌트 (CharacterController 기반)
[RequireComponent(typeof(CharacterController))]
public class PlayerStateMachine : MonoBehaviour
{
    private IPlayerState _CurrentState = null!;
    private CharacterController _CC = null!;

    // 입력 값
    private Vector3 _MovementInput = Vector3.zero;

    // 이동 속도, 중력
    private float _MoveSpeed = 5f;
    private float _Gravity = 9.8f;

    // 수직 속도
    private Vector3 _Velocity = Vector3.zero;

    void Awake()
    {
        _CC = GetComponent<CharacterController>();
    }

    void Start()
    {
        // 초기 상태: Idle
        ChangeState(new IdleState(this));
    }

    void Update()
    {
        // 1) 입력 업데이트
        float tH = Input.GetAxisRaw("Horizontal");
        float tV = Input.GetAxisRaw("Vertical");
        _MovementInput = new Vector3(tH, 0, tV);

        // 2) 기절 상태 전환 테스트 (Space)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeState(new StunState(this, 2f));
            return;
        }

        // 3) 현재 상태 로직 실행
        _CurrentState.UpdateState();
    }

    // 상태 전환은 이 메서드 하나로만!
    public void ChangeState(IPlayerState tNewState)
    {
        _CurrentState?.ExitState();
        _CurrentState = tNewState;
        _CurrentState.EnterState();
    }

    /// <summary>
    /// 수평 이동 벡터만 주면, 내부에서 중력 적용 후 Move 호출
    /// </summary>
    public void MoveWithGravity(Vector3 tHorizontalMotion)
    {
        if (_CC.isGrounded)
            _Velocity.y = -0.1f;  // 바닥 고정용
        else
            _Velocity.y -= _Gravity * Time.deltaTime;

        Vector3 tMotion = tHorizontalMotion * _MoveSpeed * Time.deltaTime;
        tMotion.y = _Velocity.y * Time.deltaTime;
        _CC.Move(tMotion);
    }

    // 외부에서 읽기 전용으로 입력 값 노출
    public Vector3 MovementInput => _MovementInput;
}

// 3) Idle 상태
public class IdleState : IPlayerState
{
    private readonly PlayerStateMachine _SM;
    public IdleState(PlayerStateMachine tSM) => _SM = tSM;

    public void EnterState()
    {
        Debug.Log("Entered Idle State");
    }

    public void UpdateState()
    {
        // 입력이 있으면 Move 상태로 전환
        if (_SM.MovementInput.magnitude > 0.1f)
            _SM.ChangeState(new MoveState(_SM));
    }

    public void ExitState()
    {
        Debug.Log("Exited Idle State");
    }
}

// 4) 이동 상태
public class MoveState : IPlayerState
{
    private readonly PlayerStateMachine _SM;
    public MoveState(PlayerStateMachine tSM) => _SM = tSM;

    public void EnterState()
    {
        Debug.Log("Entered Move State");
    }

    public void UpdateState()
    {
        Vector3 tDir = _SM.MovementInput.normalized;
        _SM.MoveWithGravity(tDir);

        // 입력이 사라지면 Idle 상태로 변경
        if (_SM.MovementInput.magnitude < 0.1f)
            _SM.ChangeState(new IdleState(_SM));
    }

    public void ExitState()
    {
        Debug.Log("Exited Move State");
    }
}

// 5) 기절 상태
public class StunState : IPlayerState
{
    private readonly PlayerStateMachine _SM;
    private float _RemainingTime;

    public StunState(PlayerStateMachine tSM, float tDuration)
    {
        _SM = tSM;
        _RemainingTime = tDuration;
    }

    public void EnterState()
    {
        Debug.Log("Entered Stun State");
    }

    public void UpdateState()
    {
        _RemainingTime -= Time.deltaTime;
        if (_RemainingTime <= 0f)
            _SM.ChangeState(new IdleState(_SM));
    }

    public void ExitState()
    {
        Debug.Log("Exited Stun State");
    }
}
