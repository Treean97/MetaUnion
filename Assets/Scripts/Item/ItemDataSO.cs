using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "ItemData", menuName = "Item/ItemData")]
public class ItemDataSO : ScriptableObject, IItemData, IBuyable, ISellable
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
    
    [SerializeField] private ScriptableObject[] _ActionSOs;
    public IInventoryAction[] Actions
    => _ActionSOs.OfType<IInventoryAction>().ToArray();

    [SerializeField] ItemDataSO _CurrencyType;
    public ItemDataSO CurrencyType => _CurrencyType;
    [SerializeField] int _BuyPrice;
    public int BuyPrice => _BuyPrice;
    [SerializeField] int _SellPrice;
    public int SellPrice => _SellPrice;

}
