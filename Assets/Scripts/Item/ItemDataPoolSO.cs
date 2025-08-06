using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataPool", menuName = "Item/ItemDataPool")]
public class ItemDataPoolSO : ScriptableObject
{
    [SerializeField] private List<ItemDataSO> _Items = new();

    private Dictionary<int, ItemDataSO> _ItemDic;

    private void Init()
    {
        if (_ItemDic != null) return;

        _ItemDic = _Items.ToDictionary(item => item.ID, item => item);
    }

    public bool TryGetItem(int id, out ItemDataSO data)
    {
        Init();
        return _ItemDic.TryGetValue(id, out data);
    }

    public int GetItemCount()
    {
        return _Items.Count;
    }

    public ItemDataSO GetItemAt(int index)
    {
        return _Items[index];
    }
}
