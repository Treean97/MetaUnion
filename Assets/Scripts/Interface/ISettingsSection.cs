public interface ISaveSection
{
    string Key { get; }         // "audio", "graphics", "input" ...
    string CaptureJson();       // 현재 상태 -> JSON
    void ApplyJson(string s); // JSON -> 상태 적용
}