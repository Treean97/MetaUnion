using UnityEngine;

namespace Controller
{
    public abstract class PlayerCamera : MonoBehaviour
    {
        private const float MIN_DISTANCE = 1f;
        private const float MAX_DISTANCE = 10f;

        private const float TARGET_DISTANCE = MAX_DISTANCE * 2f;

        protected Transform m_Player;

        [SerializeField, Range(0f, 1f)]
        private float m_SensitivityX = 0.1f;
        [SerializeField, Range(0f, 1f)]
        private float m_SensitivityY = 0.1f;

        [SerializeField, Range(0f, 1f)]
        private float m_Zoom = 0.5f;
        [SerializeField, Range(0f, 1f)]
        private float m_SensetivityZoom = 0.1f;

        [SerializeField, Range(0, 90f)]
        private float m_MinAngle = 0f;
        [SerializeField, Range(0, 90f)]
        private float m_MaxAngle = 50f;

        protected Transform m_Target;
        protected Transform m_Transform;

        protected Vector2 m_Angles;
        protected float m_Distance;

        public Vector3 Target => m_Target.position;
        public float TargetDistance => TARGET_DISTANCE;

        protected virtual void Awake()
        {
            m_Transform = transform;

            m_Target = new GameObject($"Target_{gameObject.name}").transform;
            if(m_Transform.parent != null)
            {
                m_Target.transform.parent = m_Transform.parent;
            }
        }

        public void SetPlayer(Transform player) {
            m_Player = player;
        }

        public virtual void SetInput(in Vector2 delta, float scroll)
        {
            // InputManager 옵션 반영
            Vector2 d = delta;
            float sens = 1f;
            float zoomSensGlobal = 1f;

            var im = InputManager._Inst;
            if (im != null)
            {
                if (im.IsInvertY()) d.y = -d.y;                          // Y 반전
                sens = Mathf.Max(0.01f, im.GetSensitivity());            // 시점 감도
                zoomSensGlobal = Mathf.Max(0.01f, im.GetZoomSpeed());    // 줌 속도
            }

            // 회전 (축별 기본감도 × 전역 감도)
            m_Angles += new Vector2(d.y * m_SensitivityY * sens, d.x * m_SensitivityX * sens) * 360f;
            m_Angles.x = Mathf.Clamp(m_Angles.x, m_MinAngle, m_MaxAngle);

            // 줌 (카메라 기본감도 × 전역 줌 속도)
            m_Zoom += scroll * m_SensetivityZoom * zoomSensGlobal;
            m_Zoom = Mathf.Clamp01(m_Zoom);

            m_Distance = (1f - m_Zoom) * (MAX_DISTANCE - MIN_DISTANCE) + MIN_DISTANCE;  
        }
    }
}