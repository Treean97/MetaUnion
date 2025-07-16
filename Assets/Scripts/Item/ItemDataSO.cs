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
}
