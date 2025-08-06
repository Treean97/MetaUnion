using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Item/ItemData")]
public class ItemDataSO : ScriptableObject
{
    [Header("아이템 정보")]
    [SerializeField] private int _ID;
    public int ID => _ID;
    [SerializeField] ItemInfoSO _ItemInfo;
    public ItemInfoSO ItemInfo => _ItemInfo;
    [SerializeField] Sprite _Icon;
    public Sprite Icon => _Icon;
    [SerializeField] GameObject _Prefab;
    public GameObject Prefab => _Prefab;
    
    [Tooltip("상점 구매가")]
    [SerializeField] int _Price;
    public int Price => _Price;

    [SerializeField] private ScriptableObject[] _ActionSOs;
    public IInventoryAction[] Actions
    => _ActionSOs.OfType<IInventoryAction>().ToArray();
    
}
