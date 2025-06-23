using UnityEngine;

public enum ItemType {Hair, Hat, Face };

[CreateAssetMenu(menuName = "ScriptableObject/CustomizeItem")]
public class CustomizeItemSO : ScriptableObject
{
    [SerializeField] ItemType _Type;
    public ItemType Type => _Type;
    [SerializeField] string _ID;
    public string ID => _ID;
    [SerializeField] GameObject _ItemPrefab;
}
