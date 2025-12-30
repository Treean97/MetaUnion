using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;

[Flags]
public enum InputLock
{
    None     = 0,
    Move     = 1 << 0,
    Look     = 1 << 1,
    Attack   = 1 << 2,
    Interact = 1 << 3,
    UIHotkey = 1 << 4,
}

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
        [SerializeField] private KeyCode _LeftClickKey = KeyCode.Mouse0; // 좌클릭
        [SerializeField] private KeyCode _Handkey = KeyCode.Alpha1;
        [SerializeField] private KeyCode _Axekey = KeyCode.Alpha2;
        [SerializeField] private KeyCode _Pickaxekey = KeyCode.Alpha3;
        [SerializeField] private KeyCode _ChatKey = KeyCode.T;
        [SerializeField] private KeyCode _InventoryKey = KeyCode.Q;
        [SerializeField] private KeyCode _PlayerListKey = KeyCode.Tab;
        [SerializeField] private KeyCode _CursorToggle = KeyCode.LeftAlt;
        [SerializeField] private KeyCode _EmoteKey = KeyCode.Z; // 호스트 시작만 담당

        private MoveHandler _Mover;
        private PlayerEmote _PlayerEmote;

        private Vector2 _Axis;
        private bool _IsRun;
        private bool _IsJump;

        private Vector3 _Target;
        private Vector2 _MouseDelta;
        private float _Scroll;

        private bool _IsStunnedBlocked;
        private StatusEffectManager _StatusEffectManager;

        public event Action OnInteract;

        public event Action OnSlot_0KeyPressed;
        public event Action OnSlot_1KeyPressed;
        public event Action OnSlot_2KeyPressed;

        public event Action<IWeaponState> OnWeaponChange;
        public event Action<MountInput> OnMountInput;

        private void Awake()
        {
            _Mover = GetComponent<MoveHandler>();
            _PlayerEmote = GetComponent<PlayerEmote>();

            _StatusEffectManager = GetComponent<StatusEffectManager>();
            if (_StatusEffectManager != null)
            {
                _StatusEffectManager.OnEffectApplied += HandleEffectApplied;
                _StatusEffectManager.OnEffectRemoved += HandleEffectRemoved;
            }
        }

        private void OnDestroy()
        {
            if (_StatusEffectManager != null)
            {
                _StatusEffectManager.OnEffectApplied -= HandleEffectApplied;
                _StatusEffectManager.OnEffectRemoved -= HandleEffectRemoved;
            }
        }

        private void HandleEffectApplied(StatusType type)
        {
            if (type == StatusType.Stun)
                _IsStunnedBlocked = true;
        }

        private void HandleEffectRemoved(StatusType type)
        {
            if (type == StatusType.Stun)
                _IsStunnedBlocked = false;
        }

        private bool IsInEmote()
        {
            if (!_PlayerEmote) _PlayerEmote = GetComponent<PlayerEmote>();
            return _PlayerEmote && _PlayerEmote.InEmote;
        }

        private void Update()
        {
            if (!photonView.IsMine)
                return;

            // 스턴이면 입력 전체 무시
            if (_IsStunnedBlocked)
                return;

            GatherInput();
            SetInput();
        }

        public void BindCamera(PlayerCamera cam)
        {
            _Camera = cam;
            _Camera.SetPlayer(transform);
        }

        private static bool IsPointerOverUI()
        {
            // EventSystem이 없는 씬이면 UI 판정 불가 -> UI 위가 아니라고 처리
            if (EventSystem.current == null) return false;
            return EventSystem.current.IsPointerOverGameObject();
        }

        public void GatherInput()
        {
            // 기본 수집
            _Axis = new Vector2(Input.GetAxis(_HorizontalAxis), Input.GetAxis(_VerticalAxis));
            _IsRun = Input.GetKey(_RunKey);
            _IsJump = Input.GetButton(_JumpButton);

            _Target = (_Camera == null) ? Vector3.zero : _Camera.Target;

            _MouseDelta = new Vector2(Input.GetAxis(_MouseX), Input.GetAxis(_MouseY));
            _Scroll = Input.GetAxis(_MouseScroll);

            // 커서 노출 시엔 카메라 회전 막기
            if (CursorManager._IsShown)
                _MouseDelta = Vector2.zero;

            // 락 적용
            if (InputBlockManager.IsLocked(InputLock.Move))
            {
                _Axis = Vector2.zero;
                _IsRun = false;
                _IsJump = false;
            }

            if (InputBlockManager.IsLocked(InputLock.Look))
            {
                _MouseDelta = Vector2.zero;
                _Scroll = 0f;
            }

            // 이모트 중 입력 처리
            if (IsInEmote())
            {
                // 이동/달리기/점프 차단
                _Axis = Vector2.zero;
                _IsRun = false;
                _IsJump = false;

                // 이모트 중 상호작용키 = 현재 이모트 탈출 (락과 무관하게 허용: 기존 정책 유지)
                if (Input.GetKeyDown(_InteractKey))
                    _PlayerEmote?.RequestExitByInput();

                // 나머지 행동 입력은 처리하지 않음
                return;
            }

            // 평상시 입력 처리
            // Interact
            if (!InputBlockManager.IsLocked(InputLock.Interact))
            {
                if (Input.GetKeyDown(_InteractKey))
                    OnInteract?.Invoke();
            }

            // LeftClick : UI 위 클릭이면 UI가 소비 -> 디스패처 호출 안 함
            // Attack 락이면 디스패처 호출 안 함
            if (Input.GetKeyDown(_LeftClickKey))
            {
                if (!IsPointerOverUI() && !InputBlockManager.IsLocked(InputLock.Attack))
                {
                    LeftClickDispatcher._Inst?.Dispatch();
                }
            }

            // 슬롯키
            if (Input.GetKeyDown(_Handkey)) OnSlot_0KeyPressed?.Invoke();
            if (Input.GetKeyDown(_Axekey)) OnSlot_1KeyPressed?.Invoke();
            if (Input.GetKeyDown(_Pickaxekey)) OnSlot_2KeyPressed?.Invoke();

            // UI 단축키
            if (!InputBlockManager.IsLocked(InputLock.UIHotkey))
            {
                if (Input.GetKeyDown(_InventoryKey)) UIRouter._Inst.MoveSlide<IInventoryUI>();
                if (Input.GetKeyDown(_ChatKey)) UIRouter._Inst.MoveSlide<IChatUI>();

                if (Input.GetKeyDown(_PlayerListKey)) UIRouter._Inst.Open<IPlayerListUI>();
                if (Input.GetKeyUp(_PlayerListKey)) UIRouter._Inst.Close<IPlayerListUI>();

                if (Input.GetKeyDown(_EmoteKey)) UIRouter._Inst.Open<IEmoteUI>();
                if (Input.GetKeyUp(_EmoteKey)) UIRouter._Inst.Close<IEmoteUI>();
            }

            // 커서 토글
            if (Input.GetKeyDown(_CursorToggle)) CursorManager.Toggle();

            // 차량 이동 인풋
            MountInput mountInput = new MountInput
            {
                Throttle = InputBlockManager.IsLocked(InputLock.Move) ? 0f : Input.GetAxisRaw("Vertical"),
                Steer    = InputBlockManager.IsLocked(InputLock.Move) ? 0f : Input.GetAxisRaw("Horizontal"),
                Brake    = !InputBlockManager.IsLocked(InputLock.Move) && Input.GetKey(KeyCode.Space),
            };

            OnMountInput?.Invoke(mountInput);
        }

        public void SetInput()
        {
            Vector2 axis = _Axis;
            bool isRun = _IsRun;
            bool isJump = _IsJump;

            // 이모트 중
            if (IsInEmote())
            {
                axis = Vector2.zero;
                isRun = false;
                isJump = false;
            }

            if (_Mover != null)
                _Mover.SetInput(in axis, in _Target, isRun, isJump);

            if (_Camera != null)
                _Camera.SetInput(in _MouseDelta, _Scroll);
        }
    }
}
