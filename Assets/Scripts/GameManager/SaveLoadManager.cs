using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager _Inst { get; private set; }

    string _FilePath => Path.Combine(Application.persistentDataPath, "save.json");

    // 섹션 등록표 (같은 키 재등록 시 최신으로 교체)
    readonly Dictionary<string, ISaveSection> _Sections = new();
    // 로드된 섹션별 JSON 캐시(늦게 등록된 섹션에 즉시 적용)
    readonly Dictionary<string, string> _Loaded = new();

    [System.Serializable] class Entry { public string key; public string Json; }
    [System.Serializable] class Blob { public List<Entry> Sections = new(); }

    void Awake()
    {
        if (_Inst != null) { Destroy(gameObject); return; }
        _Inst = this; DontDestroyOnLoad(gameObject);
        LoadAll();
    }

    // 종료 시 자동 저장
    void OnApplicationQuit() => SaveAll();
    
    // 등록
    public void Register(ISaveSection s)
    {
        _Sections[s.Key] = s; // 같은 키면 교체
        if (_Loaded.TryGetValue(s.Key, out var json) && !string.IsNullOrEmpty(json))
            s.ApplyJson(json); // 늦게 등록돼도 즉시 반영
    }

    // 모든 데이터 저장
    public void SaveAll()
    {
        // 파일 내용 읽기(없으면 빈 dict)
        var dict = ReadFileToDict();
        // 등록된 섹션 최신 스냅샷으로 갱신
        foreach (var kv in _Sections)
        {
            var sec = kv.Value as Object; // Unity fake-null 대응
            if (sec == null) continue;
            dict[kv.Key] = kv.Value.CaptureJson();
            _Loaded[kv.Key] = dict[kv.Key];
        }
        WriteDictToFile(dict);
    }

    // 일부 파트 저장
    public void RequestSaveSection(string key)
    {
        if (!_Sections.TryGetValue(key, out var sec)) return;
        var obj = sec as Object; if (obj == null) return;
        var dict = ReadFileToDict();
        dict[key] = sec.CaptureJson();
        _Loaded[key] = dict[key];
        WriteDictToFile(dict);
    }

    // 데이터 불러오기
    public void LoadAll()
    {
        var dict = ReadFileToDict();
        _Loaded.Clear();
        foreach (var kv in dict) _Loaded[kv.Key] = kv.Value;

        // 이미 등록된 섹션에는 즉시 적용
        foreach (var kv in _Sections)
        {
            var secObj = kv.Value as Object; if (secObj == null) continue;
            if (_Loaded.TryGetValue(kv.Key, out var json) && !string.IsNullOrEmpty(json))
                kv.Value.ApplyJson(json);
        }
    }

    // 파일을 역직렬화 후 딕셔너리 형태로 변환
    Dictionary<string, string> ReadFileToDict()
    {
        try
        {
            if (!File.Exists(_FilePath)) return new();
            var blob = JsonUtility.FromJson<Blob>(File.ReadAllText(_FilePath));
            var dict = new Dictionary<string, string>();
            if (blob?.Sections != null) foreach (var e in blob.Sections) dict[e.key] = e.Json;
            return dict;
        }
        catch { return new(); }
    }

    // 딕셔너리를 직렬화 변환 후 파일로 저장
    void WriteDictToFile(Dictionary<string, string> dict)
    {
        try
        {
            var blob = new Blob { Sections = new List<Entry>(dict.Count) };
            foreach (var kv in dict) blob.Sections.Add(new Entry{ key = kv.Key, Json = kv.Value });

            var json = JsonUtility.ToJson(blob);
            var tmp = _FilePath + ".tmp";
            File.WriteAllText(tmp, json);
            if (File.Exists(_FilePath)) File.Delete(_FilePath);
            File.Move(tmp, _FilePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[SaveLoad] write fail: {ex.Message}");
        }
    }
}
