using System.Collections.Generic;
using UnityEngine;

public class DialogueUIPanel : MonoBehaviour
{
    [SerializeField] private string _Id;   // 예: "Shop", "Inventory", "QuestResult"

    static readonly Dictionary<string, DialogueUIPanel> _Map = new();

    void Awake()
    {
        if (string.IsNullOrEmpty(_Id))
        {
            Debug.LogWarning($"[DialogueUIPanel] ID가 비어 있습니다. ({gameObject.name})");
            return;
        }

        _Map[_Id] = this;
    }

    void OnDestroy()
    {
        if (string.IsNullOrEmpty(_Id)) return;

        if (_Map.TryGetValue(_Id, out var current) && current == this)
            _Map.Remove(_Id);
    }

    public static bool TryGet(string id, out DialogueUIPanel panel)
    {
        return _Map.TryGetValue(id, out panel);
    }

    public void Show()
    {
        UIFX.Show(gameObject);   // 이미 만든 UIFX 로직 재사용
    }

    public void Hide()
    {
        UIFX.Hide(gameObject);
    }
}
