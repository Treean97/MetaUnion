using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Controller
{
    [RequireComponent(typeof(MoveHandler))]
    [RequireComponent(typeof(AttackHandler))]
    [RequireComponent(typeof(FocusHandler))]
    public class PlayerInput : MonoBehaviourPun
    {
        [Header("Character")]
        [SerializeField] private string m_HorizontalAxis = "Horizontal";
        [SerializeField] private string m_VerticalAxis = "Vertical";
        [SerializeField] private string m_JumpButton = "Jump";
        [SerializeField] private KeyCode m_RunKey = KeyCode.LeftShift;

        [Header("Camera")]
        [SerializeField] private PlayerCamera m_Camera;
        [SerializeField] private string m_MouseX = "Mouse X";
        [SerializeField] private string m_MouseY = "Mouse Y";
        [SerializeField] private string m_MouseScroll = "Mouse ScrollWheel";

        [Header("Input")]
        [SerializeField] private KeyCode m_InteractKey = KeyCode.E;
        [SerializeField] private KeyCode m_AttackKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode m_Handkey = KeyCode.Alpha1;
        [SerializeField] private KeyCode m_Axekey = KeyCode.Alpha2;
        [SerializeField] private KeyCode m_Pickaxekey = KeyCode.Alpha3;
        [SerializeField] private KeyCode m_InventoryKey = KeyCode.Q;
        [SerializeField] private KeyCode m_PlayerListKey = KeyCode.Tab;

        private MoveHandler m_Mover;

        private Vector2 m_Axis;
        private bool m_IsRun;
        private bool m_IsJump;

        private Vector3 m_Target;
        private Vector2 m_MouseDelta;
        private float m_Scroll;

        private bool _IsMovementBlocked; // 추가: 움직임 차단 플래그
        private bool _IsStunnedBlocked;
        private StatusEffectManager _StatusEffectManager;

        // 상호작용(Interact) 이벤트
        public event Action OnInteract;
        public event Action OnAttack;

        // 무기 키 입력 이벤트
        public event Action OnHandKeyPressed;
        public event Action OnAxeKeyPressed;
        public event Action OnPickaxeKeyPressed;

        // 무기 상태 변경 이벤트
        public event Action<IWeaponState> OnWeaponChange;

        private void Awake()
        {
            m_Mover = GetComponent<MoveHandler>();

            InputBlock.OnInputBlockStatus += HandleUIRunningStateChanged; // 추가: 구독

            _StatusEffectManager = GetComponent<StatusEffectManager>();
            if (_StatusEffectManager != null)
            {
                _StatusEffectManager.OnEffectApplied += HandleEffectApplied;
                _StatusEffectManager.OnEffectRemoved += HandleEffectRemoved;
            }

            // 키 입력 이벤트 구독
            OnHandKeyPressed     += () => OnWeaponChange?.Invoke(new HandState());
            OnAxeKeyPressed      += () => OnWeaponChange?.Invoke(new AxeState());
            OnPickaxeKeyPressed  += () => OnWeaponChange?.Invoke(new PickaxeState());
        }

        private void OnDestroy()
        {
            InputBlock.OnInputBlockStatus -= HandleUIRunningStateChanged; // 추가: 구독 해제
            if (_StatusEffectManager != null)
            {
                _StatusEffectManager.OnEffectApplied -= HandleEffectApplied;
                _StatusEffectManager.OnEffectRemoved -= HandleEffectRemoved;
            }
        }

        private void HandleUIRunningStateChanged(bool isBlocked)
        {
            _IsMovementBlocked = isBlocked; // 추가: UI가 활성화되면 움직임 차단

            // 움직임 0으로 
            m_Axis = Vector2.zero;

        }

        // 기절이 걸렸을 때
        private void HandleEffectApplied(StatusType type)
        {
            if (type == StatusType.Stun)
                _IsStunnedBlocked = true;
        }


        // 기절 해제될 때
        private void HandleEffectRemoved(StatusType type)
        {
            if (type == StatusType.Stun)
                _IsStunnedBlocked = false;
        }


        private void Update()
        {
            if (_IsMovementBlocked || _IsStunnedBlocked) return; // 추가: 차단 상태 시 입력 방지
            if(!photonView.IsMine)
            {
                return;
            }
            GatherInput();
            SetInput();
        }

        public void BindCamera(PlayerCamera cam)
        {
            m_Camera = cam;
            m_Camera.SetPlayer(transform);
        }


        public void GatherInput()
        {
            m_Axis = new Vector2(Input.GetAxis(m_HorizontalAxis), Input.GetAxis(m_VerticalAxis));
            m_IsRun = Input.GetKey(m_RunKey);
            m_IsJump = Input.GetButton(m_JumpButton);

            m_Target = (m_Camera == null) ? Vector3.zero : m_Camera.Target;
            m_MouseDelta = new Vector2(Input.GetAxis(m_MouseX), Input.GetAxis(m_MouseY));
            m_Scroll = Input.GetAxis(m_MouseScroll);

            if (Input.GetKeyDown(m_InteractKey))
            {
                OnInteract?.Invoke();
            }

            if (Input.GetKeyDown(m_AttackKey))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                return;
                
                OnAttack?.Invoke();
            }

            if (Input.GetKeyDown(m_Handkey))
            {
                OnHandKeyPressed?.Invoke();
            }

            if (Input.GetKeyDown(m_Axekey))
            {
                OnAxeKeyPressed?.Invoke();
            }

            if (Input.GetKeyDown(m_Pickaxekey))
            {
                OnPickaxeKeyPressed?.Invoke();
            }

            if (Input.GetKeyDown(m_InventoryKey))
            {
                GameEvents.RaiseRequestToggleInventoryUI();
            }

            if (Input.GetKeyDown(m_PlayerListKey))
            {
                GameEvents.RaiseRequestOpenPlayerListUI();
            }

            if (Input.GetKeyUp(m_PlayerListKey))
            {
                GameEvents.RaiseRequestClosePlayerListUI();
            }
        }

        public void SetInput()
        {
            if (m_Mover != null)
            {
                m_Mover.SetInput(in m_Axis, in m_Target, m_IsRun, m_IsJump);
            }

            if (m_Camera != null)
            {
                m_Camera.SetInput(in m_MouseDelta, m_Scroll);
            }
        }
        
        
    }
}
