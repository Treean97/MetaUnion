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
        [SerializeField] private KeyCode _EmoteKey = KeyCode.Z; // 호스트 시작만 담당

        private MoveHandler _Mover;
        private PlayerEmote _PlayerEmote;

        private Vector2 _Axis;
        private bool _IsRun;
        private bool _IsJump;

        private Vector3 _Target;
        private Vector2 _MouseDelta;
        private float _Scroll;

        private bool _IsMovementBlocked;
        private bool _IsStunnedBlocked;
        private StatusEffectManager _StatusEffectManager;

        public event Action OnInteract;
        public event Action OnAttack;

        public event Action OnSlot_0KeyPressed;
        public event Action OnSlot_1KeyPressed;
        public event Action OnSlot_2KeyPressed;

        public event Action<IWeaponState> OnWeaponChange;

        private void Awake()
        {
            _Mover = GetComponent<MoveHandler>();
            _PlayerEmote = GetComponent<PlayerEmote>();

            InputBlockManager.OnInputBlockStatus += HandleUIRunningStateChanged;

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
            _IsMovementBlocked = isBlocked;

            if (!photonView.IsMine) return;

            if (isBlocked)
            {
                _Axis = Vector2.zero;
                _IsRun = false;
                _IsJump = false;
                _MouseDelta = Vector2.zero;
                SetInput();
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

        bool IsInEmote()
        {
            if (!_PlayerEmote) _PlayerEmote = GetComponent<PlayerEmote>();
            return _PlayerEmote && _PlayerEmote.InEmote;
        }

        private void Update()
        {
            if (_IsMovementBlocked || _IsStunnedBlocked)
                return;

            if (!photonView.IsMine)
                return;

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

            if (CursorManager._IsShown)
                _MouseDelta = Vector2.zero;

            if (Input.GetKeyDown(_InteractKey))
                OnInteract?.Invoke();

            if (Input.GetKeyDown(_AttackKey))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                    OnAttack?.Invoke();
            }

            if (Input.GetKeyDown(_Handkey)) OnSlot_0KeyPressed?.Invoke();
            if (Input.GetKeyDown(_Axekey)) OnSlot_1KeyPressed?.Invoke();
            if (Input.GetKeyDown(_Pickaxekey)) OnSlot_2KeyPressed?.Invoke();

            if (Input.GetKeyDown(_InventoryKey)) UIRouter._Inst.Toggle<IInventoryUI>();
            if (Input.GetKeyDown(_ChatKey)) UIRouter._Inst.Toggle<IChatUI>();

            if (Input.GetKeyDown(_PlayerListKey)) UIRouter._Inst.Open<IPlayerListUI>();
            if (Input.GetKeyUp(_PlayerListKey)) UIRouter._Inst.Close<IPlayerListUI>();

            if (Input.GetKeyDown(_CursorToggle)) CursorManager.Toggle();

            // Z 키로 _Catalog[0] 이모트 주최 시작(테스트용)
            if (Input.GetKeyDown(_EmoteKey))
            {
                // UIRouter._Inst.Open<IEmoteUI>();
                if (!photonView.IsMine) return;

                var mgr = EmoteManager._Inst;
                if (mgr == null || mgr.EmoteSOs == null || mgr.EmoteSOs.Length == 0) { Debug.LogWarning("[EmoteTest] EmoteSO 리스트 비어있음"); return; }

                var so = mgr.EmoteSOs[0];
                if (!so || !so.EmoteAnchor) { Debug.LogWarning("[EmoteTest] EmoteSO 또는 EmoteAnchor 누락"); return; }

                var pe = GetComponent<PlayerEmote>();
                if (!pe) { Debug.LogWarning("[EmoteTest] PlayerEmote 컴포넌트 없음"); return; }

                // 앵커 생성(생성과 동시에 0번 슬롯 예약)
                Vector3 pos = transform.position + transform.forward * 1.5f;
                Quaternion rot = Quaternion.LookRotation(-transform.forward, Vector3.up);
                var anchor = mgr.StartEmote(so, pos, rot, pe);
                if (!anchor) return;

                // 바로 재생 RPC (0번 슬롯, 정규화 시간은 막 시작했으니 0에 가깝지만 공식대로 계산)
                float nt = EmoteManager.GetNormalizedTime(anchor);
                pe.photonView.RPC(nameof(PlayerEmote.RPC_PlayEmote), RpcTarget.All, anchor.photonView.ViewID, 0, nt);
                
            }    
        }

        public void SetInput()
        {
            Vector2 axis = _Axis;
            bool isRun   = _IsRun;
            bool isJump  = _IsJump;

            if (IsInEmote())
            {
                axis  = Vector2.zero;
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
