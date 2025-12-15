using System;
using System.Collections.Generic;
using System.IO;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using Object = UnityEngine.Object;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager _Inst { get; private set; }

    string _FilePath => Path.Combine(Application.persistentDataPath, "save.json");

    // 섹션 등록표 (같은 키 재등록 시 최신으로 교체)
    readonly Dictionary<string, ILocalSaveSection> _LocalSections = new();
    // 로드된 섹션별 JSON 캐시(늦게 등록된 섹션에 즉시 적용)
    readonly Dictionary<string, string> _LocalLoaded = new();

    [Serializable] class Entry { public string key; public string Json; }
    [Serializable] class Blob { public List<Entry> Sections = new(); }

    // 클라우드 저장 (PlayFab UserData)
    readonly Dictionary<string, ICloudSaveSection> _CloudSections = new();
    readonly Dictionary<string, string> _CloudLoaded = new(); // 서버에서 읽은 원본 캐시
    public bool CloudReady => PlayFabClientAPI.IsClientLoggedIn();
    // 종료 시 중복 저장 방지
    bool _DidSaveOnQuit;

    void Awake()
    {
        if (_Inst != null && _Inst != this)
        {
            Destroy(this);
            return;
        }
        _Inst = this;
        DontDestroyOnLoad(gameObject);

        LoadAllLocal();

        PlayfabLoginManager.OnLoginSuccess += OnLoginSuccessCloud;
        Application.wantsToQuit += OnWantsToQuit;
        if (CloudReady) OnLoginSuccessCloud();
    }

    void OnDestroy()
    {
        PlayfabLoginManager.OnLoginSuccess -= OnLoginSuccessCloud;
        Application.wantsToQuit -= OnWantsToQuit;
    }

    #region 로컬 저장
    // 종료 시 자동 저장

    bool OnWantsToQuit()
    {
        if (_DidSaveOnQuit) return true;
        _DidSaveOnQuit = true;

        SaveAllLocal();
        return true;
    }

    void OnApplicationQuit()
    {
        if (_DidSaveOnQuit) return;
        _DidSaveOnQuit = true;

        SaveAllLocal();
    }

    // 등록
    public void RegisterLocal(ILocalSaveSection s)
    {
        _LocalSections[s.Key] = s; // 같은 키면 교체
        if (_LocalLoaded.TryGetValue(s.Key, out var json) && !string.IsNullOrEmpty(json))
            s.ApplyJson(json); // 늦게 등록돼도 즉시 반영
    }

    public void UnregisterLocal(ILocalSaveSection s)
    {
        if (s == null) return;
        if (_LocalSections.TryGetValue(s.Key, out var cur) && ReferenceEquals(cur, s))
            _LocalSections.Remove(s.Key);
    }

    public void SaveAllLocal()
    {
        // 기존 파일을 기반으로 갱신
        var dict = ReadFileToDict();

        foreach (var kv in _LocalSections)
        {
            var secObj = kv.Value as Object;

            // 종료 도중 Destroy돼서 null이면 기존 dict 값만 유지
            if (secObj == null) 
                continue;

            var snap = kv.Value.CaptureJson();

            if (string.IsNullOrEmpty(snap))
            {
                dict.Remove(kv.Key);
                _LocalLoaded.Remove(kv.Key);
            }
            else
            {
                dict[kv.Key] = snap;
                _LocalLoaded[kv.Key] = snap;
            }
        }

        WriteDictToFile(dict);
    }


    public void SaveLocalSection(string key)
    {
        if (!_LocalSections.TryGetValue(key, out var sec)) return;
        var obj = sec as Object; if (obj == null) return;

        // 현재 파일을 읽어서 갱신/삭제
        var dict = ReadFileToDict();
        var snap = sec.CaptureJson();

        if (string.IsNullOrEmpty(snap))
        {
            // null/빈 값이면 키 삭제
            dict.Remove(key);
            _LocalLoaded.Remove(key);
        }
        else
        {
            dict[key] = snap;
            _LocalLoaded[key] = snap;
        }

        WriteDictToFile(dict);
    }

    // 데이터 불러오기
    public void LoadAllLocal()
    {
        var dict = ReadFileToDict();
        _LocalLoaded.Clear();
        foreach (var kv in dict) _LocalLoaded[kv.Key] = kv.Value;

        // 이미 등록된 섹션에는 즉시 적용
        foreach (var kv in _LocalSections)
        {
            var secObj = kv.Value as Object; if (secObj == null) continue;
            if (_LocalLoaded.TryGetValue(kv.Key, out var json) && !string.IsNullOrEmpty(json))
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
            foreach (var kv in dict) blob.Sections.Add(new Entry { key = kv.Key, Json = kv.Value });

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
    #endregion
    #region 클라우드 저장
    void OnLoginSuccessCloud()
    {
        // 로그인 직후 전체 클라우드 로드
        LoadAllCloud();
    }

    public void RegisterCloud(ICloudSaveSection s)
    {
        _CloudSections[s.Key] = s;
        // 이미 클라우드가 로드돼 있다면 즉시 반영
        if (_CloudLoaded.TryGetValue(s.Key, out var json) && !string.IsNullOrEmpty(json))
            s.ApplyJson(json);
    }

    public void UnregisterCloud(ICloudSaveSection s)
    {
        if (s == null) return;
        if (_CloudSections.TryGetValue(s.Key, out var cur) && ReferenceEquals(cur, s))
            _CloudSections.Remove(s.Key);
    }

    public void LoadAllCloud()
    {
        if (!CloudReady) { Debug.LogWarning("Not logged in"); return; }

        var keys = new List<string>(_CloudSections.Keys);
        var req = new GetUserDataRequest { Keys = (keys.Count > 0 ? keys : null) };

        PlayFabClientAPI.GetUserData(req, r =>
        {
            _CloudLoaded.Clear();
            if (r.Data != null)
            {             
                foreach (var kv in r.Data)
                {
                    _CloudLoaded[kv.Key] = kv.Value?.Value;   
                }                    
            }

            foreach (var kv in _CloudSections)
            {
                if (_CloudLoaded.TryGetValue(
                    kv.Key, out var json) && !string.IsNullOrEmpty(json))
                {
                    kv.Value.ApplyJson(json);
                }
            }              
                    
            Debug.Log("LoadAllCloud Success");
        }, e => Debug.LogError($"LoadAllCloud fail: {e.GenerateErrorReport()}"));
    }


    // 단일 키 로드
    public void LoadCloud(string key, Action<string> ok = null, Action<string> err = null)
    {
        if (!CloudReady) { err?.Invoke("Not logged in"); return; }
        var req = new GetUserDataRequest { Keys = new List<string> { key } };
        PlayFabClientAPI.GetUserData(req, r =>
        {
            string v = null;
            if (r.Data != null && r.Data.TryGetValue(key, out var data))
            {
                v = data.Value;  
            } 
            _CloudLoaded[key] = v;

            // 등록된 섹션에 즉시 적용
            if (_CloudSections.TryGetValue(key, out var sec) && !string.IsNullOrEmpty(v))
            {
                sec.ApplyJson(v);
            }                
            ok?.Invoke(v);
        },
        e => { Debug.LogError(e.GenerateErrorReport()); err?.Invoke(e.GenerateErrorReport()); });
    }

    // 저장
    public void SaveCloud(string key, string json, Action ok = null, Action<string> err = null)
    {
        if (!CloudReady) { err?.Invoke("Not logged in"); return; }

        if (string.IsNullOrEmpty(json))
        {
            var delReq = new UpdateUserDataRequest { KeysToRemove = new List<string> { key } };
            PlayFabClientAPI.UpdateUserData(delReq, _ => { _CloudLoaded.Remove(key); ok?.Invoke(); },
                e => { Debug.LogError(e.GenerateErrorReport()); err?.Invoke(e.GenerateErrorReport()); });
            return;
        }

        var req = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { key, json } },
            Permission = UserDataPermission.Private
        };
        PlayFabClientAPI.UpdateUserData(req, _ => { _CloudLoaded[key] = json; ok?.Invoke(); },
            e => { Debug.LogError(e.GenerateErrorReport()); err?.Invoke(e.GenerateErrorReport()); });
    }

    // 등록된 섹션을 이용한 저장(섹션이 스스로 CaptureJson 호출)
    public void SaveCloudSection(string key, Action ok = null, Action<string> err = null)
    {
        if (!_CloudSections.TryGetValue(key, out var sec))
        {
            err?.Invoke($"No cloud section: {key}");
            return;
        }
        var json = sec.CaptureJson();
        SaveCloud(key, json, ok, err);
    }
    #endregion
}
