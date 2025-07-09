// Reward.cs  (Assets/Scripts/Core/Reward.cs)
public enum ResourceType
{
    Item,
    Currency
}

public struct ResourceChange
{
    public ResourceType Category;
    public string         Key;
    public int            Amount;

    public ResourceChange(ResourceType category, string key, int amount)
    {
        Category = category;
        Key      = key;
        Amount   = amount;
    }
}
