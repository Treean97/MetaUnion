using Controller;
using Photon.Pun;
using UnityEngine;

public class FocusHandler : MonoBehaviourPun
{
    [SerializeField] private float _Distance = 2f;
    [SerializeField] private float _YOffset = 1f;
    [SerializeField] private Vector3 _BoxHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);
    [SerializeField] private LayerMask _LayerMask;

    private PlayerInput _PlayerInput;
    private IFocusable _CurrentFocus;
    private IInteractionReceiver _CurrentInteractionReceiver;

    void Awake()
    {
        _PlayerInput = GetComponent<PlayerInput>();
        _PlayerInput.OnInteract += HandleInteract;
    }

    void OnDestroy()
    {
        _PlayerInput.OnInteract -= HandleInteract;
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        // 물리 동기화
        Physics.SyncTransforms(); 

        Vector3 boxCenter = transform.position 
                    + transform.forward * (_Distance * 0.5f) 
                    + transform.up * _YOffset;

        Quaternion orientation = transform.rotation;
        Vector3 half = new Vector3(_BoxHalfExtents.x, _BoxHalfExtents.y, Mathf.Max(_BoxHalfExtents.z, _Distance * 0.5f));

        Collider[] hits = Physics.OverlapBox(boxCenter, half, orientation, _LayerMask);

        IFocusable closestFocusable = null;
        float closestDistance = float.PositiveInfinity;

        foreach (var collider in hits)
        {
            var focusable = collider.GetComponent<IFocusable>();
            if (focusable != null)
            {
                float dist = Vector3.Distance(transform.position, collider.ClosestPoint(transform.position));
                if (dist < closestDistance)
                {
                    closestFocusable = focusable;
                    closestDistance = dist;
                }
            }
        }

        if (closestFocusable != _CurrentFocus)
        {
            _CurrentFocus?.OnDefocus();
            _CurrentFocus = closestFocusable;
            _CurrentFocus?.OnFocus();
        }

        if (hits.Length == 0 && _CurrentFocus != null)
        {
            _CurrentFocus.OnDefocus();
            _CurrentFocus = null;
        }
    }

    private void HandleInteract()
    {
        if (!photonView.IsMine) return;

        if (_CurrentFocus is IInteractable interactable)
        {
            // 🔹 상호작용 대상이 IInteractionReceiver도 구현했다면
            _CurrentInteractionReceiver = _CurrentFocus as IInteractionReceiver;
            if (_CurrentInteractionReceiver != null)
            {
                // 플레이어 Transform 전달 → NPC가 이 방향으로 회전
                _CurrentInteractionReceiver.BeginInteraction(transform);
            }

            // 원래 하던 상호작용 실행 (대화 시작 등)
            interactable.OnInteract();

            // 🔹 대화 끝났을 때 EndInteraction 호출하도록 훅 등록
            if (DialogueManager._Inst != null)
            {
                DialogueManager._Inst.OnEnd -= HandleDialogueEnd; // 중복 등록 방지
                DialogueManager._Inst.OnEnd += HandleDialogueEnd;
            }
        }
    }
    
    private void HandleDialogueEnd()
    {
        // 상호작용 대상이 있었다면 EndInteraction 호출
        if (_CurrentInteractionReceiver != null)
        {
            _CurrentInteractionReceiver.EndInteraction();
            _CurrentInteractionReceiver = null;
        }

        // 다 썼으면 이벤트 해제
        if (DialogueManager._Inst != null)
        {
            DialogueManager._Inst.OnEnd -= HandleDialogueEnd;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 boxCenter = transform.position
                        + transform.forward * (_Distance * 0.5f)
                        + transform.up * _YOffset;
        Quaternion orientation = transform.rotation;
        Vector3 half = new Vector3(_BoxHalfExtents.x, _BoxHalfExtents.y,
                                Mathf.Max(_BoxHalfExtents.z, _Distance * 0.5f));

        Gizmos.matrix = Matrix4x4.TRS(boxCenter, orientation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, half * 2f);
    }
}
