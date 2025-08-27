using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryContextMenuUIManager : MonoBehaviour
{
    [SerializeField] private GameObject _InventoryContextMenuUI;
    [SerializeField] private GameObject _ButtonPrefab;
    [SerializeField] private Transform _Container;

    void Update()
    {
        // 좌클릭 시, 메뉴 "밖"을 눌렀을 때만 닫기
        if (Input.GetMouseButtonDown(0) && !IsPointerOverMenu())
        {
            _InventoryContextMenuUI.SetActive(false);
        }
    }

    public void SetContextMenu(Dictionary<string, Action> options, Vector2 pos)
    {
        // 1) 기존 옵션 제거
        foreach (Transform child in _Container)
            Destroy(child.gameObject);

        // 2) 새 옵션 생성
        foreach (var option in options)
        {
            var go = Instantiate(_ButtonPrefab, _Container);
            var text = go.GetComponentInChildren<TMP_Text>();
            var btn = go.GetComponent<Button>();

            text.text = option.Key;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                option.Value?.Invoke();
                _InventoryContextMenuUI.SetActive(false);
            });
        }

        // 3) 위치 설정 & 표시
        _InventoryContextMenuUI.transform.position = pos;
    }
    
    private bool IsPointerOverMenu()
    {
        if (EventSystem.current == null) return false;

        var eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var r in results)
        {
            var go = r.gameObject;
            if (go == null) continue;

            if (go == _InventoryContextMenuUI) return true;
            if (go.transform.IsChildOf(_InventoryContextMenuUI.transform)) return true;
        }
        return false;
    }
}
