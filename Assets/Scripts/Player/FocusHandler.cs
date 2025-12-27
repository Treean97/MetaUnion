using System.Collections.Generic;
using Controller;
using Photon.Pun;
using UnityEngine;

public class FocusHandler : MonoBehaviourPun
{
    [SerializeField] private float _Distance = 2f;
    [SerializeField] private float _YOffset = 1f;
    [SerializeField] private Vector3 _BoxHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);
    [SerializeField] private LayerMask _LayerMask;

    [Header("NonAlloc")]
    [SerializeField] private int _MaxHits = 16;

    private readonly Dictionary<Collider, IFocusable> _FocusableCache = new();
    private Collider[] _Hits;

    private PlayerInput _PlayerInput;
    private IFocusable _CurrentFocus;
    private IInteractionReceiver _CurrentInteractionReceiver;

    void Awake()
    {
        _PlayerInput = GetComponent<PlayerInput>();
        _PlayerInput.OnInteract += HandleInteract;

        _Hits = new Collider[Mathf.Max(1, _MaxHits)];
    }

    void OnDestroy()
    {
        _PlayerInput.OnInteract -= HandleInteract;
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        Physics.SyncTransforms();

        Vector3 boxCenter = transform.position
                        + transform.forward * (_Distance * 0.5f)
                        + transform.up * _YOffset;

        Quaternion orientation = transform.rotation;
        Vector3 half = new Vector3(_BoxHalfExtents.x, _BoxHalfExtents.y, Mathf.Max(_BoxHalfExtents.z, _Distance * 0.5f));

        int hitCount = Physics.OverlapBoxNonAlloc(boxCenter, half, _Hits, orientation, _LayerMask);

        IFocusable closestFocusable = null;
        float closestDistSqr = float.PositiveInfinity;

        Vector3 originPos = transform.position;

        for (int i = 0; i < hitCount; i++)
        {
            var col = _Hits[i];
            if (col == null) continue;

            if (!_FocusableCache.TryGetValue(col, out var focusable) || focusable == null)
            {
                focusable = FindHelper.FindInterfaceInParent<IFocusable>(col.transform, includeSelf: true);
                _FocusableCache[col] = focusable;
            }

            if (focusable == null) continue;

            Vector3 p = col.ClosestPoint(originPos);
            float distSqr = (originPos - p).sqrMagnitude;

            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closestFocusable = focusable;
            }
        }

        if (closestFocusable != _CurrentFocus)
        {
            _CurrentFocus?.OnDefocus();
            _CurrentFocus = closestFocusable;
            _CurrentFocus?.OnFocus();
        }
    }

    private void HandleInteract()
    {
        if (!photonView.IsMine) return;

        if (_CurrentFocus is IInteractable interactable)
        {
            _CurrentInteractionReceiver = _CurrentFocus as IInteractionReceiver;
            if (_CurrentInteractionReceiver != null)
                _CurrentInteractionReceiver.BeginInteraction(transform);

            interactable.OnInteract();

            if (DialogueManager._Inst != null)
            {
                DialogueManager._Inst.OnEnd -= HandleDialogueEnd;
                DialogueManager._Inst.OnEnd += HandleDialogueEnd;
            }
        }
    }

    private void HandleDialogueEnd()
    {
        if (_CurrentInteractionReceiver != null)
        {
            _CurrentInteractionReceiver.EndInteraction();
            _CurrentInteractionReceiver = null;
        }

        if (DialogueManager._Inst != null)
            DialogueManager._Inst.OnEnd -= HandleDialogueEnd;
    }
}
