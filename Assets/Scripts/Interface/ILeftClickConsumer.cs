public interface ILeftClickConsumer
{
    /// true면 소비, false면 다음 후보에게 넘김
    bool ConsumeLeftClick();
}