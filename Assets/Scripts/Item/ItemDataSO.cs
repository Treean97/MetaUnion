using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Item/ItemData")]
public class ItemDataSO : ScriptableObject
{
    [Header("아이템 정보")]
    [SerializeField] private int _ID;
    public int ID => _ID;
    [SerializeField] ItemInfoSO _ItemInfo;
    public ItemInfoSO ItemInfo => _ItemInfo;

    // [Header("리소스")]
    // public Sprite _Icon;
    // public GameObject _Prefab;
}
