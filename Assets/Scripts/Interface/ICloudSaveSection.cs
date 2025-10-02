public interface ICloudSaveSection
{
    string Key { get; }
    string CaptureJson();
    void ApplyJson(string s);
}
