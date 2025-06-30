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
        [SerializeField] private float m_WalkSpeed = 1f;
        [SerializeField] private float m_RunSpeed = 4f;
        [SerializeField, Range(0f, 360f)] private float m_RotateSpeed = 90f;
        [SerializeField] private Space m_Space = Space.Self;
        [SerializeField] private float m_JumpHeight = 5f;

        [Header("Animator")]
        [SerializeField] private string m_HorizontalID = "Hor";
        [SerializeField] private string m_VerticalID = "Vert";
        [SerializeField] private string m_StateID = "State";
        [SerializeField] private string m_JumpID = "IsJump";
        [SerializeField] private LookWeight m_LookWeight = new LookWeight(1f, 0.3f, 0.7f, 1f);

        [SerializeField] private Transform m_GroundCheck;
        [SerializeField] private float m_CheckRadius = 0.2f;

        private Transform m_Transform;
        private CharacterController m_Controller;
        private Animator m_Animator;
        private PhotonView _PhotonView;

        private MovementHandler m_Movement;
        private AnimationHandler m_Animation;

        private Vector2 m_Axis;
        private Vector3 m_Target;
        private bool m_IsRun;
        private bool m_IsJump;
        private bool m_IsMoving;

        public Vector2 Axis => m_Axis;
        public Vector3 Target => m_Target;
        public bool IsRun => m_IsRun;

        private void OnValidate()
        {
            m_WalkSpeed = Mathf.Max(m_WalkSpeed, 0f);
            m_RunSpeed = Mathf.Max(m_RunSpeed, m_WalkSpeed);
            m_Movement?.SetStats(m_WalkSpeed, m_RunSpeed, m_RotateSpeed, m_JumpHeight, m_Space);
        }

        private void Awake()
        {
            // 컴포넌트 초기화
            m_Transform = transform;
            m_Controller = GetComponent<CharacterController>();
            m_Animator = GetComponent<Animator>();
            _PhotonView = GetComponent<PhotonView>();


            // 핸들러 초기화
            m_Movement = new MovementHandler(m_Controller, m_Transform, m_WalkSpeed, m_RunSpeed, m_RotateSpeed, m_JumpHeight, m_Space, m_GroundCheck, m_CheckRadius);
            m_Animation = new AnimationHandler(m_Animator, m_HorizontalID, m_VerticalID, m_StateID, m_JumpID);
        }

        void Start()
        {            
            // PlayerStat에서 값을 가져와 이동 속성 설정
            var stat = GetComponent<PlayerStat>();
            if (stat != null)
            {
                m_WalkSpeed = stat.GetBaseStat(StatType.MoveSpeed);
                m_RunSpeed = stat.GetBaseStat(StatType.RunSpeed);
                m_JumpHeight = stat.GetBaseStat(StatType.JumpPower);
            }
        }

        private void Update()
        {
            if (!_PhotonView.IsMine) return;

            m_Movement.Move(Time.deltaTime, in m_Axis, in m_Target, m_IsRun, m_IsJump, m_IsMoving, out var animAxis, out var isAir);
            m_Animation.Animate(in animAxis, m_IsRun ? 1f : 0f, isAir, Time.deltaTime);
        }

        private void OnAnimatorIK()
        {
            m_Animation.AnimateIK(in m_Target, m_LookWeight);
        }

        public void SetInput(in Vector2 axis, in Vector3 target, in bool isRun, in bool isJump)
        {
            m_Axis = axis;
            m_Target = target;
            m_IsRun = isRun;
            m_IsJump = isJump;
            m_IsMoving = m_Axis.sqrMagnitude >= Mathf.Epsilon;
            if (!m_IsMoving) m_Axis = Vector2.zero;
            else m_Axis = Vector2.ClampMagnitude(m_Axis, 1f);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.normal.y > m_Controller.stepOffset)
                m_Movement.SetSurface(hit.normal);
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

        #region Handlers
        private class MovementHandler
        {
            private readonly CharacterController m_Controller;
            private readonly Transform m_Transform;
            private float m_WalkSpeed, m_RunSpeed, m_RotateSpeed, m_JumpHeight;
            private Space m_Space;
            private readonly float m_Luft = 75f;
            private readonly float m_JumpReload = 1f;
            private float m_TargetAngle;
            private bool m_IsRotating;
            private Vector3 m_Normal;
            private Vector3 m_GravityAcceleration = Physics.gravity;
            private float m_jumpTimer;
            private readonly Transform m_GroundCheck;
            private readonly float m_CheckRadius;
            private readonly float m_IgnoreGroundTime = 0.1f;
            private float m_IgnoreTimer;

            public MovementHandler(CharacterController controller, Transform transform, float walkSpeed, float runSpeed, float rotateSpeed, float jumpHeight, Space space, Transform groundCheck, float checkRadius)
            {
                m_Controller = controller;
                m_Transform = transform;
                m_WalkSpeed = walkSpeed;
                m_RunSpeed = runSpeed;
                m_RotateSpeed = rotateSpeed;
                m_JumpHeight = jumpHeight;
                m_Space = space;
                m_GroundCheck = groundCheck;
                m_CheckRadius = checkRadius;
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

            public void Move(float deltaTime, in Vector2 axis, in Vector3 target, bool isRun, bool isJump, bool isMoving, out Vector2 animAxis, out bool isAir)
            {
                var targetForward = (target - m_Transform.position).normalized;
                ConvertMovement(in axis, in targetForward, out var movement);
                CaculateGravity(isJump, deltaTime, out isAir);
                Displace(deltaTime, in movement, isRun);
                Turn(in targetForward, isMoving);
                UpdateRotation(deltaTime);
                GenAnimationAxis(in movement, out animAxis);
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

            private void Displace(float deltaTime, in Vector3 movement, bool isRun)
            {
                var displacement = (isRun ? m_RunSpeed : m_WalkSpeed) * movement + m_GravityAcceleration;
                m_Controller.Move(displacement * deltaTime);
            }

            private void CaculateGravity(bool isJump, float deltaTime, out bool isAir)
            {
                m_jumpTimer = Mathf.Max(m_jumpTimer - deltaTime, 0f);
                if (m_IgnoreTimer > 0f) m_IgnoreTimer -= deltaTime;

                if (IsGrounded())
                {
                    if (isJump && m_jumpTimer <= 0f)
                    {
                        var gravity = Physics.gravity;
                        var mag = gravity.magnitude;
                        m_GravityAcceleration += -(gravity / mag) * Mathf.Sqrt(m_JumpHeight * 6f * mag);
                        m_jumpTimer = m_JumpReload;
                        m_IgnoreTimer = m_IgnoreGroundTime;
                        isAir = true;
                        return;
                    }
                    if (m_IgnoreTimer > 0f)
                    {
                        isAir = true;
                        return;
                    }
                    m_GravityAcceleration = Physics.gravity;
                    isAir = false;
                    return;
                }
                isAir = true;
                m_GravityAcceleration += Physics.gravity * deltaTime;
            }

            private bool IsGrounded()
            {
                if (m_IgnoreTimer > 0f) return false;
                return Physics.CheckSphere(m_GroundCheck.position, m_CheckRadius, LayerMask.GetMask("Ground"));
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
                var rotDelta = m_RotateSpeed * deltaTime;
                if (rotDelta + Mathf.PI * 2f + Mathf.Epsilon >= Mathf.Abs(m_TargetAngle))
                {
                    rotDelta = m_TargetAngle;
                    m_IsRotating = false;
                }
                else rotDelta *= Math.Sign(m_TargetAngle);
                m_Transform.Rotate(Vector3.up, rotDelta);
            }
        }

        private class AnimationHandler
        {
            private readonly Animator m_Animator;
            private readonly string m_HorizontalID;
            private readonly string m_VerticalID;
            private readonly string m_StateID;
            private readonly string m_JumpID;
            private readonly float k_InputFlow = 4.5f;
            private float m_FlowState;
            private Vector2 m_FlowAxis;

            public AnimationHandler(Animator animator, string horizontalID, string verticalID, string stateID, string jumpID)
            {
                m_Animator = animator;
                m_HorizontalID = horizontalID;
                m_VerticalID = verticalID;
                m_StateID = stateID;
                m_JumpID = jumpID;
            }

            public void Animate(in Vector2 axis, float state, bool isJump, float deltaTime)
            {
                m_Animator.SetFloat(m_HorizontalID, m_FlowAxis.x);
                m_Animator.SetFloat(m_VerticalID, m_FlowAxis.y);
                m_Animator.SetFloat(m_StateID, Mathf.Clamp01(m_FlowState));
                m_Animator.SetBool(m_JumpID, isJump);
                m_FlowAxis = Vector2.ClampMagnitude(m_FlowAxis + k_InputFlow * deltaTime * (axis - m_FlowAxis).normalized, 1f);
                m_FlowState = Mathf.Clamp01(m_FlowState + k_InputFlow * deltaTime * Math.Sign(state - m_FlowState));
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
