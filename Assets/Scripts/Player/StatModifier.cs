public class StatModifier
{
    public StatType Type;
    public float AddValue;    // 덧셈형
    public float MulFactor;   // 곱셈형 (예: 1.3)
    public float Duration;    // 지속시간(초), 0이면 영구
    internal float ExpireTime;
}
