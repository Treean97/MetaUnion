using UnityEngine;

public interface IItemData
{
    int ID { get; }
    Sprite Icon { get; }
    GameObject Prefab { get; }
}