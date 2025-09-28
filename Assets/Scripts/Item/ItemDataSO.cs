using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "ItemData", menuName = "Item/ItemData")]
public class ItemDataSO : ScriptableObject, IItemData
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
    [SerializeField] int _BuyPrice;
    public int BuyPrice => _BuyPrice;

    [Tooltip("상점 판매가")]
    [SerializeField] int _SellPrice;
    public int SellPrice => _SellPrice;

    [Tooltip("구매/판매에 사용할 통화")]
    [SerializeField] private ItemDataSO _PriceCurrency;
    public ItemDataSO PriceCurrency => _PriceCurrency;

    [SerializeField] private ScriptableObject[] _ActionSOs;
    public IInventoryAction[] Actions
    => _ActionSOs.OfType<IInventoryAction>().ToArray();
    
}
