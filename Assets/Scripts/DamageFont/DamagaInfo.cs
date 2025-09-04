[System.Flags]
public enum DamageTool
{
    None    = 0,
    Hand    = 1 << 0,
    Axe     = 1 << 1,
    Pickaxe = 1 << 2,
}

public struct DamageInfo
{
    public float damage;
    public DamageTool tool;
}
