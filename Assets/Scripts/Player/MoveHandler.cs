using System;
using UnityEngine;
using Photon.Pun;

namespace Controller
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(PlayerStat))]
    [DisallowMultipleComponent]
    public class MoveHandler : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _WalkSpeed;
        [SerializeField] private float _RunSpeed;
        [SerializeField, Range(0f, 360f)] private float _RotateSpeed = 90f;
        [SerializeField] private Space _Space = Space.Self;
        [SerializeField] private float _JumpHeight;
        [SerializeField] private PlayerStat _PlayerStat;

        [Header("Animator")]
        [SerializeField] private string _HorizontalID = "Hor";
        [SerializeField] private string _VerticalID = "Vert";
        [SerializeField] private string _StateID = "State";
        [SerializeField] private string _JumpTriggerID = "JumpTrigger";
        [SerializeField] private string _IsGroundedID = "IsGrounded";
        [SerializeField] private LookWeight _LookWeight = new LookWeight(1f, 0.3f, 0.7f, 1f);

        // Raycast를 위한 변수
        [Header("Ground Check")]
        [SerializeField] private Transform _FootTransform;
        [SerializeField] private float _LandingCheckDistance = 0.5f;

        private Transform _Transform;
        private CharacterController _Controller;
        private Animator _Animator;
        private PhotonView _PhotonView;
        private PlayerEmote _PlayerEmote;
        private MovementHandler _Movement;
        private AnimationHandler _Animation;

        private Vector2 _Axis;
        private Vector3 _Target;
        private bool _IsRun;
        private bool _IsJump;
        private bool _IsMoving;

        private bool _IsFalling;
        private bool _LandingTriggered = false;

        public Vector2 Axis => _Axis;
        public Vector3 Target => _Target;
        public bool IsRun => _IsRun;

        int _PhaseLockCount;


        private void OnValidate()
        {
            _WalkSpeed = Mathf.Max(_WalkSpeed, 0f);
            _RunSpeed = Mathf.Max(_RunSpeed, _WalkSpeed);
            _Movement?.SetStats(_WalkSpeed, _RunSpeed, _RotateSpeed, _JumpHeight, _Space);
        }

        private void Awake()
        {
            _Transform = transform;
            _Controller = GetComponent<CharacterController>();
            _Animator = GetComponent<Animator>();
            _PhotonView = GetComponent<PhotonView>();
            _PlayerEmote = GetComponent<PlayerEmote>();

            _Movement = new MovementHandler(_Controller, _Transform, _WalkSpeed, _RunSpeed, _RotateSpeed, _JumpHeight, _Space);
            _Animation = new AnimationHandler(_Animator, _HorizontalID, _VerticalID, _StateID, _JumpTriggerID, _IsGroundedID);

            PlayerStat.OnStatChanged += HandleStatChanged;
        }

        void OnDestroy()
        {
            PlayerStat.OnStatChanged -= HandleStatChanged;
        }

        void OnEnable()
        {
            _PlayerEmote.OnEmoteStart += HandleLockTurn;
            _PlayerEmote.OnEmoteEnd += HandleUnlockTurn;
            FishingSequence.OnFishingStart += HandleLockTurn;
            FishingSequence.OnFishingEnd += HandleUnlockTurn;
        }

        void OnDisable()
        {
            _PlayerEmote.OnEmoteStart -= HandleLockTurn;
            _PlayerEmote.OnEmoteEnd -= HandleUnlockTurn;
            FishingSequence.OnFishingStart -= HandleLockTurn;
            FishingSequence.OnFishingEnd -= HandleUnlockTurn;
        }

        void Start()
        {
            UpdateMovementStats();
        }

        private void HandleStatChanged(StatType type, float newValue)
        {
            if (type == StatType.MoveSpeed || type == StatType.RunSpeed || type == StatType.JumpPower)
                UpdateMovementStats();
        }

        private void UpdateMovementStats()
        {
            float walk = _PlayerStat.GetStat(StatType.MoveSpeed);
            float run = _PlayerStat.GetStat(StatType.RunSpeed);
            float jump = _PlayerStat.GetStat(StatType.JumpPower);

            _Movement.SetStats(walk, run, _RotateSpeed, jump, _Space);
        }

        private void Update()
        {
            if (!_PhotonView.IsMine) return;

            bool jumpInput = _IsJump;
            _IsJump = false;

            // IsGrounded 상태와 애니메이션 컨트롤은 계속해서 동기화
            _Animation.Animate(in _Axis, _IsRun ? 1f : 0f, Time.deltaTime, _Controller.isGrounded);

            // 수정: 점프 로직은 CharacterController.isGrounded에 의존
            if (_Controller.isGrounded)
            {
                _IsFalling = false;
                _LandingTriggered = false; // 착지 후 트리거 상태 초기화
                _Animation.SetJumpEnd(false);

                if (jumpInput)
                {
                    _Animation.SetJumpTrigger();
                }
            }
            else // 공중에 있을 때
            {
                // Y축 속도가 음수일 때 낙하 시작으로 판단
                if (_Movement.VerticalVelocity.y < 0)
                {
                    _IsFalling = true;
                }

                // 낙하 중이며, 착지 애니메이션이 아직 발동되지 않았을 때
                if (_IsFalling && !_LandingTriggered)
                {
                    RaycastHit hit;

                    // 수정: 레이어 마스크를 제거한 Raycast
                    if (Physics.Raycast(_FootTransform.position, Vector3.down, out hit, _LandingCheckDistance))
                    {
                        Debug.Log("착지 애니메이션 시작 IsJumpEnd: true");
                        _Animation.SetJumpEnd(true);
                        _LandingTriggered = true;
                    }
                }
            }

            _Movement.Move(Time.deltaTime, in _Axis, in _Target, _IsRun, jumpInput, _IsMoving, _Controller.isGrounded, out var animAxis);
        }


        private void OnAnimatorIK()
        {
            _Animation.AnimateIK(in _Target, _LookWeight);
        }

        public void SetInput(in Vector2 axis, in Vector3 target, in bool isRun, in bool isJump)
        {
            _Axis = axis;
            _Target = target;
            _IsRun = isRun;
            _IsMoving = _Axis.sqrMagnitude >= Mathf.Epsilon;
            if (!_IsMoving) _Axis = Vector2.zero;
            else _Axis = Vector2.ClampMagnitude(_Axis, 1f);
            
            if (isJump)
                _IsJump = true;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.normal.y > _Controller.stepOffset)
                _Movement.SetSurface(hit.normal);
        }

        [Serializable]
        private struct LookWeight
        {
            public float weight, body, head, eyes;
            public LookWeight(float weight, float body, float head, float eyes)
            {
                this.weight = weight;
                this.body = body;
                this.head = head;
                this.eyes = eyes;
            }
        }

        public void HandleLockTurn()
        {
            if (!_PhotonView || !_PhotonView.IsMine) return;
            _PhaseLockCount++;
            if (_PhaseLockCount == 1)
                _Movement._IsLockTurn = true;
        }
        
        public void HandleUnlockTurn()
        {
            if (!_PhotonView || !_PhotonView.IsMine) return;
            _PhaseLockCount = Mathf.Max(0, _PhaseLockCount - 1);
            if (_PhaseLockCount == 0)
                _Movement._IsLockTurn = false;
        }

        #region Handlers
        private class MovementHandler
        {
            private readonly CharacterController m_Controller;
            private readonly Transform m_Transform;
            private float m_WalkSpeed, m_RunSpeed, m_RotateSpeed, m_JumpHeight;
            private Space m_Space;
            private readonly float m_Luft = 75f;
            private float m_TargetAngle;
            private bool m_IsRotating;
            private Vector3 m_Normal;
            private Vector3 m_VerticalVelocity;
            internal bool _IsLockTurn;            

            public Vector3 VerticalVelocity => m_VerticalVelocity;

            public MovementHandler(CharacterController controller, Transform transform, float walkSpeed, float runSpeed, float rotateSpeed, float jumpHeight, Space space)
            {
                m_Controller = controller;
                m_Transform = transform;
                m_WalkSpeed = walkSpeed;
                m_RunSpeed = runSpeed;
                m_RotateSpeed = rotateSpeed;
                m_JumpHeight = jumpHeight;
                m_Space = space;
            }

            public void SetStats(float walkSpeed, float runSpeed, float rotateSpeed, float jumpHeight, Space space)
            {
                m_WalkSpeed = walkSpeed;
                m_RunSpeed = runSpeed;
                m_RotateSpeed = rotateSpeed;
                m_JumpHeight = jumpHeight;
                m_Space = space;
            }

            public void SetSurface(in Vector3 normal) => m_Normal = normal;

            public void Move(float deltaTime, in Vector2 axis, in Vector3 target, bool isRun, bool isJump, bool isMoving, bool isGrounded, out Vector2 animAxis)
            {
                CalculateGravity(isGrounded, isJump, deltaTime);

                var targetForward = (target - m_Transform.position).normalized;
                ConvertMovement(in axis, in targetForward, out var horizontalMovement);

                var displacement = (isRun ? m_RunSpeed : m_WalkSpeed) * horizontalMovement + m_VerticalVelocity;
                Displace(deltaTime, in displacement);

                Turn(in targetForward, isMoving);
                UpdateRotation(deltaTime);
                GenAnimationAxis(in horizontalMovement, out animAxis);
            }

            private void ConvertMovement(in Vector2 axis, in Vector3 targetForward, out Vector3 movement)
            {
                Vector3 forward = m_Space == Space.Self
                    ? new Vector3(targetForward.x, 0f, targetForward.z).normalized
                    : Vector3.forward;
                Vector3 right = m_Space == Space.Self
                    ? Vector3.Cross(Vector3.up, forward).normalized
                    : Vector3.right;
                movement = Vector3.ProjectOnPlane(axis.x * right + axis.y * forward, m_Normal);
            }

            private void Displace(float deltaTime, in Vector3 displacement)
            {
                m_Controller.Move(displacement * deltaTime);
            }

            private void CalculateGravity(bool isGrounded, bool isJump, float deltaTime)
            {
                if (isGrounded)
                {
                    if (isJump)
                    {
                        float v0 = Mathf.Sqrt(2f * Physics.gravity.magnitude * m_JumpHeight);
                        m_VerticalVelocity.y = v0;
                    }
                    else
                    {
                        m_VerticalVelocity.y = -2f;
                    }
                }
                else
                {
                    m_VerticalVelocity.y += Physics.gravity.y * deltaTime;
                }
            }

            private void GenAnimationAxis(in Vector3 movement, out Vector2 animAxis)
            {
                if (m_Space == Space.Self)
                {
                    animAxis = new Vector2(
                        Vector3.Dot(movement, m_Transform.right),
                        Vector3.Dot(movement, m_Transform.forward)
                    );
                }
                else
                {
                    animAxis = new Vector2(
                        Vector3.Dot(movement, Vector3.right),
                        Vector3.Dot(movement, Vector3.forward)
                    );
                }
            }

            private void Turn(in Vector3 targetForward, bool isMoving)
            {
                if (_IsLockTurn) return;

                var angle = Vector3.SignedAngle(
                    m_Transform.forward,
                    Vector3.ProjectOnPlane(targetForward, Vector3.up),
                    Vector3.up
                );
                if (!m_IsRotating)
                {
                    if (!isMoving && Mathf.Abs(angle) < m_Luft)
                        return;
                    m_IsRotating = true;
                }
                m_TargetAngle = angle;
            }

            private void UpdateRotation(float deltaTime)
            {
                if (!m_IsRotating) return;

                float step = m_RotateSpeed * deltaTime;
                float remain = Mathf.Abs(m_TargetAngle);

                if (step + Mathf.Epsilon >= remain)
                {
                    m_Transform.Rotate(Vector3.up, m_TargetAngle);
                    m_IsRotating = false;
                    m_TargetAngle = 0f;
                    return;
                }

                float signed = step * Math.Sign(m_TargetAngle);
                m_Transform.Rotate(Vector3.up, signed);
                m_TargetAngle -= signed; 
            }
        }

        private class AnimationHandler
        {
            private readonly Animator m_Animator;
            private readonly string m_HorizontalID;
            private readonly string m_VerticalID;
            private readonly string m_StateID;
            private readonly string m_JumpTriggerID;
            private readonly string m_IsGroundedID;
            private readonly float k_InputFlow = 4.5f;
            private float m_FlowState;
            private Vector2 m_FlowAxis;
            
            private string m_IsJumpEndID = "IsJumpEnd";

            public AnimationHandler(Animator animator, string horizontalID, string verticalID, string stateID, string jumpTriggerID, string isGroundedID)
            {
                m_Animator = animator;
                m_HorizontalID = horizontalID;
                m_VerticalID = verticalID;
                m_StateID = stateID;
                m_JumpTriggerID = jumpTriggerID;
                m_IsGroundedID = isGroundedID;
            }

            public void Animate(in Vector2 axis, float state, float deltaTime, bool isGrounded)
            {
                m_Animator.SetFloat(m_HorizontalID, m_FlowAxis.x);
                m_Animator.SetFloat(m_VerticalID, m_FlowAxis.y);
                m_Animator.SetFloat(m_StateID, Mathf.Clamp01(m_FlowState));

                m_Animator.SetBool(m_IsGroundedID, isGrounded);

                m_FlowAxis = Vector2.ClampMagnitude(m_FlowAxis + k_InputFlow * deltaTime * (axis - m_FlowAxis).normalized, 1f);
                m_FlowState = Mathf.Clamp01(m_FlowState + k_InputFlow * deltaTime * Math.Sign(state - m_FlowState));
            }

            public void SetJumpTrigger()
            {
                if (m_Animator.gameObject.GetComponent<PhotonView>().IsMine)
                {
                    m_Animator.SetTrigger(m_JumpTriggerID);
                }
            }

            public void SetJumpEnd(bool value)
            {
                if (m_Animator.gameObject.GetComponent<PhotonView>().IsMine)
                {
                    m_Animator.SetBool(m_IsJumpEndID, value);
                }
            }

            public void AnimateIK(in Vector3 target, in LookWeight lookWeight)
            {
                m_Animator.SetLookAtPosition(target);
                m_Animator.SetLookAtWeight(lookWeight.weight, lookWeight.body, lookWeight.head, lookWeight.eyes);
            }
        }
        #endregion
    }
}