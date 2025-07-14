using UnityEngine;

[CreateAssetMenu(fileName = "ItemInfo", menuName = "Item/ItemInfo")]
public class ItemInfoSO : ScriptableObject
{
    [Header("아이템 이름")]
    public string _DisplayName;

    [Header("아이템 설명")]
    [TextArea] public string _Description;

}
