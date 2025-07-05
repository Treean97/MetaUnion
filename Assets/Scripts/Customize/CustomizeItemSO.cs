using UnityEngine;

public enum ItemType {Hair, Hat, Face, Accessory, Glasses, Outwear, Gloves, Pants, Socks, Shoes };

[CreateAssetMenu(menuName = "ScriptableObject/CustomizeItem")]
public class CustomizeItemSO : ScriptableObject
{
    [SerializeField] ItemType _Type;
    public ItemType Type => _Type;
    [SerializeField] string _ID;
    public string ID => _ID;
    [SerializeField] Mesh _ItemMesh;
    public Mesh ItemMesh => _ItemMesh;
    [SerializeField] int _Price;
    public int Price => _Price;

    [SerializeField] private bool _IsDefaultUnlocked;
    public bool IsDefaultUnlocked => _IsDefaultUnlocked;
}
