using System.Collections.Generic;

[System.Serializable]
public class CustomizeItemGroup
{
    public ItemType Type;                   // Hair, Hat, Face, …
    public List<CustomizeItemSO> Items;     // 해당 타입의 아이템들
    
}

