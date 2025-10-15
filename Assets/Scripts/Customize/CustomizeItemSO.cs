using UnityEngine;

public enum ItemType {Hair, Hat, Face, Accessory, Glasses, Outwear, Gloves, Pants, Shoes };

[CreateAssetMenu(menuName = "Customize/CustomizeItem")]
public class CustomizeItemSO : ScriptableObject
{
    [SerializeField] ItemType _Type;
    public ItemType Type => _Type;
    [SerializeField] InfoDataSO _InfoDataSO;
    public InfoDataSO InfoDataSO => _InfoDataSO;
    [SerializeField] string _ID;
    public string ID => _ID;
    [SerializeField] Mesh _ItemMesh;
    public Mesh ItemMesh => _ItemMesh;
    [SerializeField] Sprite _Sprite;
    public Sprite Sprite => _Sprite;
    [SerializeField] ItemDataSO _CurrencyType;
    public ItemDataSO CurrencyType => _CurrencyType;
    [SerializeField] int _BuyPrice;
    public int BuyPrice => _BuyPrice;

    [SerializeField] private bool _IsDefaultUnlocked;
    public bool IsDefaultUnlocked => _IsDefaultUnlocked;
}
