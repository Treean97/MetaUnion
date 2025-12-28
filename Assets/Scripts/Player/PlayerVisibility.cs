using UnityEngine;

public sealed class PlayerVisibility : MonoBehaviour
{
    [SerializeField] private Renderer[] _Renderers;

    void Awake()
    {
        if (_Renderers == null || _Renderers.Length == 0)
            _Renderers = GetComponentsInChildren<Renderer>(true);
    }

    public void SetVisible(bool visible)
    {
        if (_Renderers == null) return;

        for (int i = 0; i < _Renderers.Length; i++)
        {
            var r = _Renderers[i];
            if (!r) continue;
            r.enabled = visible;
        }
    }
}
