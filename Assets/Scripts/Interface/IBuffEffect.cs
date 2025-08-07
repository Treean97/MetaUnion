public interface IBuffEffect
{
    /// <summary>버프 적용 시 호출</summary>
    /// <param name="playerStat">플레이어 스탯 시스템</param>
    /// <param name="buffData">버프 데이터 자체</param>
    void Apply(PlayerStat playerStat, BuffDataSO buffData);
}