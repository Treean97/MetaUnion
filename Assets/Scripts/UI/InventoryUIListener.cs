using System;
using System.Collections;
using UnityEngine;

public class InventoryUIListener : MonoBehaviour
{
    [SerializeField] private RectTransform _InventoryUI;
    [SerializeField] private Vector3 _OpenPosition;
    [SerializeField] private Vector3 _ClosePosition;
    [SerializeField] private float _Duration = 0.5f;

    private bool _IsRunning = false;
    private bool _IsOpen = false;

    void OnEnable()
    {
        GameEvents.OnRequestToggleInventoryUI += HandleToggleInventory;
    }

    void OnDisable()
    {
        GameEvents.OnRequestToggleInventoryUI -= HandleToggleInventory;
    }

    void HandleToggleInventory()
    {
        if (_IsRunning) return;

        Vector2 start = _IsOpen ? _OpenPosition : _ClosePosition;
        Vector2 end = _IsOpen ? _ClosePosition : _OpenPosition;

        StartCoroutine(UIMoveCoroutine(start, end));
        _IsOpen = !_IsOpen;
    }

    IEnumerator UIMoveCoroutine(Vector3 start, Vector3 end)
    {
        _IsRunning = true;
        float elapsed = 0f;
        while (elapsed < _Duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _Duration);
            _InventoryUI.anchoredPosition = Vector3.Lerp(start, end, t);
            yield return null;
        }

       _InventoryUI.anchoredPosition = end;

        _IsRunning = false;
    }
}
