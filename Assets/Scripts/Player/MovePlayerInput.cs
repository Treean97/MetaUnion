using UnityEngine;

namespace Controller
{
    [RequireComponent(typeof(CharacterMover))]
    public class MovePlayerInput : MonoBehaviour
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

        private CharacterMover m_Mover;

        private Vector2 m_Axis;
        private bool m_IsRun;
        private bool m_IsJump;

        private Vector3 m_Target;
        private Vector2 m_MouseDelta;
        private float m_Scroll;

        private bool _IsMovementBlocked; // 추가: 움직임 차단 플래그

        private void Awake()
        {
            m_Mover = GetComponent<CharacterMover>();

            GameEvents.OnChatIsRunning += HandleUIRunningStateChanged; // 추가: 구독
        }

        private void OnDestroy()
        {
            GameEvents.OnChatIsRunning -= HandleUIRunningStateChanged; // 추가: 구독 해제
        }

        private void HandleUIRunningStateChanged(bool isRunning)
        {
            _IsMovementBlocked = isRunning; // 추가: UI가 활성화되면 움직임 차단
        }

        private void Update()
        {
            if (_IsMovementBlocked) return; // 추가: 차단 상태 시 입력 방지

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
