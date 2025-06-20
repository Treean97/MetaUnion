using UnityEngine;

public class PlayerRay : MonoBehaviour
{
    [SerializeField] private float _Distance = 2f;
    [SerializeField] private float _YOffset = 1f;
    [SerializeField] private Vector3 _BoxHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);
    [SerializeField] private LayerMask _LayerMask;

    private IFocusable _CurrentFocus;

    void Update()
    {
        Vector3 boxCenter = transform.position + transform.forward * _Distance * 0.5f + transform.up * _YOffset;
        Quaternion orientation = transform.rotation;

        Collider[] hits = Physics.OverlapBox(boxCenter, _BoxHalfExtents, orientation, _LayerMask);

        IFocusable closestFocusable = null;
        float closestDistance = _Distance;

        foreach (var collider in hits)
        {
            var focusable = collider.GetComponent<IFocusable>();
            if (focusable != null)
            {
                float dist = Vector3.Distance(transform.position, collider.transform.position);
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

        if (_CurrentFocus is IInteractable interactable && Input.GetKeyDown(KeyCode.E))
        {
            interactable.OnInteract();
        }

        if (hits.Length == 0 && _CurrentFocus != null)
        {
            _CurrentFocus.OnDefocus();
            _CurrentFocus = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 사각형 시야 시각화
        Gizmos.color = Color.cyan;
        Vector3 boxCenter = transform.position + transform.forward * _Distance * 0.5f + transform.up * _YOffset;
        Gizmos.matrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, _BoxHalfExtents * 2f);
    }
}
