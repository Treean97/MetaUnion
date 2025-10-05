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
        [SerializeField] private string _HorizontalAxis = "Horizontal";
        [SerializeField] private string _VerticalAxis = "Vertical";
        [SerializeField] private string _JumpButton = "Jump";
        [SerializeField] private KeyCode _RunKey = KeyCode.LeftShift;

        [Header("Camera")]
        [SerializeField] private PlayerCamera _Camera;
        [SerializeField] private string _MouseX = "Mouse X";
        [SerializeField] private string _MouseY = "Mouse Y";
        [SerializeField] private string _MouseScroll = "Mouse ScrollWheel";

        [Header("Input")]
        [SerializeField] private KeyCode _InteractKey = KeyCode.E;
        [SerializeField] private KeyCode _AttackKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode _Handkey = KeyCode.Alpha1;
        [SerializeField] private KeyCode _Axekey = KeyCode.Alpha2;
        [SerializeField] private KeyCode _Pickaxekey = KeyCode.Alpha3;
        [SerializeField] private KeyCode _ChatKey = KeyCode.T;
        [SerializeField] private KeyCode _InventoryKey = KeyCode.Q;
        [SerializeField] private KeyCode _PlayerListKey = KeyCode.Tab;
        [SerializeField] private KeyCode _CursorToggle = KeyCode.LeftAlt;
        [SerializeField] private KeyCode _EmoteKey = KeyCode.Z;

        private MoveHandler _Mover;

        private Vector2 _Axis;
        private bool _IsRun;
        private bool _IsJump;

        private Vector3 _Target;
        private Vector2 _MouseDelta;
        private float _Scroll;

        private bool _IsMovementBlocked; // 추가: 움직임 차단 플래그
        private bool _IsStunnedBlocked;
        private StatusEffectManager _StatusEffectManager;

        // 상호작용(Interact) 이벤트
        public event Action OnInteract;
        public event Action OnAttack;

        // 무기 키 입력 이벤트
        public event Action OnSlot_0KeyPressed;
        public event Action OnSlot_1KeyPressed;
        public event Action OnSlot_2KeyPressed;

        // 무기 상태 변경 이벤트
        public event Action<IWeaponState> OnWeaponChange;
                

        private void Awake()
        {
            _Mover = GetComponent<MoveHandler>();

            InputBlockManager.OnInputBlockStatus += HandleUIRunningStateChanged; // 추가: 구독

            _StatusEffectManager = GetComponent<StatusEffectManager>();
            if (_StatusEffectManager != null)
            {
                _StatusEffectManager.OnEffectApplied += HandleEffectApplied;
                _StatusEffectManager.OnEffectRemoved += HandleEffectRemoved;
            }
        }

        private void OnDestroy()
        {
            InputBlockManager.OnInputBlockStatus -= HandleUIRunningStateChanged;

            if (_StatusEffectManager != null)
            {
                _StatusEffectManager.OnEffectApplied -= HandleEffectApplied;
                _StatusEffectManager.OnEffectRemoved -= HandleEffectRemoved;
            }
            
        }

        private void HandleUIRunningStateChanged(bool isBlocked)
        {
            _IsMovementBlocked = isBlocked; // 추가: UI가 활성화되면 움직임 차단

            if (!photonView.IsMine) return;

            if (isBlocked)
            {
                // 움직임 0으로 
                _Axis = Vector2.zero;
                _IsRun = false;
                _IsJump = false;
                _MouseDelta = Vector2.zero;

                // 반영
                SetInput();                
            }

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
            // 차단 상태 시 모든 입력 방지
            // 이동 중이라면 정지 시키기
            if (_IsMovementBlocked || _IsStunnedBlocked)
            {
                return;
            }
            
            if(!photonView.IsMine)
            {
                return;
            }

            GatherInput();
            SetInput();
        }

        public void BindCamera(PlayerCamera cam)
        {
            _Camera = cam;
            _Camera.SetPlayer(transform);
        }


        public void GatherInput()
        {
            _Axis = new Vector2(Input.GetAxis(_HorizontalAxis), Input.GetAxis(_VerticalAxis));
            _IsRun = Input.GetKey(_RunKey);
            _IsJump = Input.GetButton(_JumpButton);

            _Target = (_Camera == null) ? Vector3.zero : _Camera.Target;
            _MouseDelta = new Vector2(Input.GetAxis(_MouseX), Input.GetAxis(_MouseY));
            _Scroll = Input.GetAxis(_MouseScroll);

            // 커서 켜졌을 때는 회전 잠금
            if (CursorManager._IsShown)
            {
                _MouseDelta = Vector2.zero;
            }

            if (Input.GetKeyDown(_InteractKey))
            {
                OnInteract?.Invoke();
            }

            if (Input.GetKeyDown(_AttackKey))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;

                OnAttack?.Invoke();
            }

            if (Input.GetKeyDown(_Handkey))
            {
                OnSlot_0KeyPressed?.Invoke();
            }

            if (Input.GetKeyDown(_Axekey))
            {
                OnSlot_1KeyPressed?.Invoke();
            }

            if (Input.GetKeyDown(_Pickaxekey))
            {
                OnSlot_2KeyPressed?.Invoke();
            }

            if (Input.GetKeyDown(_InventoryKey))
            {
                UIRouter._Inst.Toggle<IInventoryUI>();
            }

            if (Input.GetKeyDown(_ChatKey))
            {
                UIRouter._Inst.Toggle<IChatUI>();
            }

            if (Input.GetKeyDown(_PlayerListKey))
            {
                UIRouter._Inst.Open<IPlayerListUI>();
            }

            if (Input.GetKeyUp(_PlayerListKey))
            {
                UIRouter._Inst.Close<IPlayerListUI>();
            }

            if (Input.GetKeyDown(_CursorToggle))
            {
                CursorManager.Toggle();
            }

            if (Input.GetKeyDown(_EmoteKey))
            {
                UIRouter._Inst.Open<IEmoteUI>();
            }

            if (Input.GetKeyUp(_EmoteKey))
            {
                UIRouter._Inst.Close<IEmoteUI>();
            }
        }

        public void SetInput()
        {
            if (_Mover != null)
            {
                _Mover.SetInput(in _Axis, in _Target, _IsRun, _IsJump);
            }

            if (_Camera != null)
            {
                _Camera.SetInput(in _MouseDelta, _Scroll);
            }
        }
        
        
    }
}
