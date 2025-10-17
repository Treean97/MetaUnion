using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Audio;


// 저장용
[System.Serializable]
public struct AudioSettingsDTO
{
    public float Master;  // 0~1
    public float BGM;     // 0~1
    public float SFX;     // 0~1
}


public class AudioManager : MonoBehaviour, ILocalSaveSection
{
    public static AudioManager _Inst { get; private set; }

    public string Key => "audio";
    const string P_Master = "Master";
    const string P_BGM = "BGM";
    const string P_SFX = "SFX";

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup _Master;
    [SerializeField] private AudioMixerGroup _BGMGroup;
    [SerializeField] private AudioMixerGroup _SFXGroup;

    [Header("Pooled Players (Prefabs)")]
    [SerializeField] private Pooled2DAudioPlayer _SFX2DPlayerPrefab; // spatialBlend=0
    [SerializeField] private Pooled3DAudioPlayer _SFX3DPlayerPrefab; // spatialBlend=1

    [SerializeField] private SoundSO[] _SoundDatas;

    AudioMixer Mixer => _Master ? _Master.audioMixer :
                        _BGMGroup ? _BGMGroup.audioMixer :
                        _SFXGroup ? _SFXGroup.audioMixer : null;

    private AudioSource _BGMSource;   // BGM 전용

    private bool _SFXBlock;
    public void SFXBlock() => _SFXBlock = true;
    public void SFXUnBlock() => _SFXBlock = false;

    // BGM Runtime Mute 
    readonly HashSet<string> _BGMMuteTokens_RT = new();
    Coroutine _BGMRuntimeFadeCo;
    float _BGMRuntimeMult = 1f; // 0~1, _BGMSource.volume에 곱해짐


    float _MasterValue = 1f, _BGMValue = 1f, _SFXValue = 1f;

    readonly Dictionary<int, SoundSO> _Map = new();

    // DB로 변환
    static float ToDB(float v) => v <= 0.0001f ? -80f : Mathf.Log10(v) * 20f;

    void Awake()
    {
        if (_Inst != null && _Inst != this)
        {
            Destroy(this);
            return;
        }
        _Inst = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // BGM 소스
        _BGMSource = gameObject.AddComponent<AudioSource>();
        _BGMSource.playOnAwake = false;
        _BGMSource.loop = true;
        _BGMSource.spatialBlend = 0f; // 2D
        _BGMSource.outputAudioMixerGroup = _BGMGroup;

        SaveLoadManager._Inst?.RegisterLocal(this);
    }

    public float GetMasterValue() => _MasterValue;
    public float GetBGMValue() => _BGMValue;
    public float GetSFXValue() => _SFXValue;

    public void SetMasterValue(float v, bool requestSave = true)
    {
        _MasterValue = Mathf.Clamp01(v);
        if (Mixer) Mixer.SetFloat(P_Master, ToDB(_MasterValue));
        if (requestSave) SaveLoadManager._Inst?.SaveLocalSection(Key);
    }

    public void SetBGMValue(float v, bool requestSave = true)
    {
        _BGMValue = Mathf.Clamp01(v);
        if (Mixer) Mixer.SetFloat(P_BGM, ToDB(_BGMValue));
        if (requestSave) SaveLoadManager._Inst?.SaveLocalSection(Key);
    }

    public void SetSFXValue(float v, bool requestSave = true)
    {
        _SFXValue = Mathf.Clamp01(v);
        if (Mixer) Mixer.SetFloat(P_SFX, ToDB(_SFXValue));
        if (requestSave) SaveLoadManager._Inst?.SaveLocalSection(Key);
    }

    public void PlayBGM(AudioClip clip)
    {
        if (!clip) return;
        if (_BGMSource.clip == clip) return;
        _BGMSource.clip = clip;
        _BGMSource.volume = _BGMRuntimeMult;
        _BGMSource.Play();
    }

    public void BeginBGMMuteRuntime(string token, float fadeSec = 0.1f)
    {
        if (string.IsNullOrEmpty(token)) return;
        _BGMMuteTokens_RT.Add(token);
        StartBGMRuntimeFade(0f, fadeSec);
    }
    public void EndBGMMuteRuntime(string token, float fadeSec = 0.1f)
    {
        if (string.IsNullOrEmpty(token)) return;
        _BGMMuteTokens_RT.Remove(token);
        float target = _BGMMuteTokens_RT.Count > 0 ? 0f : 1f;
        StartBGMRuntimeFade(target, fadeSec);
    }
    void StartBGMRuntimeFade(float to, float fadeSec)
    {
        if (_BGMRuntimeFadeCo != null) StopCoroutine(_BGMRuntimeFadeCo);
        _BGMRuntimeFadeCo = StartCoroutine(CoBGMRuntimeFade(Mathf.Clamp01(to), Mathf.Max(0.01f, fadeSec)));
    }

    IEnumerator CoBGMRuntimeFade(float to, float fade)
    {
        float from = _BGMRuntimeMult;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fade;
            _BGMRuntimeMult = Mathf.Lerp(from, to, t);
            if (_BGMSource) _BGMSource.volume = _BGMRuntimeMult; // Mixer 값과 독립
            yield return null;
        }
        _BGMRuntimeMult = to;
        if (_BGMSource) _BGMSource.volume = _BGMRuntimeMult;
        _BGMRuntimeFadeCo = null;
    }


    // === SFX 유틸: 키 → Entry 조회 ===
    bool TryResolve(string key, out SoundSO.Entry e)
    {
        e = null;
        if (string.IsNullOrEmpty(key) || _SoundDatas == null) return false;
        for (int i = 0; i < _SoundDatas.Length; i++)
        {
            var so = _SoundDatas[i];
            if (so != null && so.TryGet(key, out e)) return true;
        }
        return false;
    }

    // === SFX 길이 조회(첫 유효 클립 기준) ===
    public bool TryGetAudioLengthByKey(string key, out float length)
    {
        length = 0f;
        if (!TryResolve(key, out var e)) return false;
        if (e.Clips != null)
        {
            for (int i = 0; i < e.Clips.Length; i++)
            {
                var c = e.Clips[i];
                if (c) { length = c.length; return length > 0.0001f; }
            }
        }
        return false;
    }

    public Pooled2DAudioPlayer Play2DLoopLocalPlayByKey(string key)
    {
        if (_SFXBlock || string.IsNullOrEmpty(key)) return null;

        SoundSO.Entry e = null;

        if (_SoundDatas != null)
        {
            for (int i = 0; i < _SoundDatas.Length; i++)
            {
                var so = _SoundDatas[i];
                if (so != null && so.TryGet(key, out e)) break;
            }
        }
        if (e == null) { Debug.LogWarning($"[Audio] key not found: {key}"); return null; }

        // 반드시 2D로만 루프
        if (e.Space != SoundSpace.S2D)
        {
            Debug.LogWarning($"[Audio] '{key}' is not S2D. Use PlayAttachedByKey for 3D loops.");
            return null;
        }

        var clip = e.PickClip(); if (!clip) return null;

        var p2d = ObjectPoolManager._Inst?.Rent(_SFX2DPlayerPrefab);
        if (!p2d) { Debug.LogWarning("[Audio] 2D pool not ready"); return null; }

        p2d.ConfigureMixer(_SFXGroup);
        // 루프 강제: e.Loop와 무관하게 루프 돌립니다.
        p2d.Play(clip, e.Volume, true);

        return p2d; // p2d.StopAndReturn() 으로 중단    
    }

    public Pooled2DAudioPlayer Play2DLoopFromOffsetByKey(string key, float offsetSec)
    {
        if (_SFXBlock || string.IsNullOrEmpty(key)) return null;
        if (!TryResolve(key, out var e)) { Debug.LogWarning($"[Audio] key not found: {key}"); return null; }
        if (e.Space != SoundSpace.S2D) { Debug.LogWarning($"[Audio] '{key}' is not S2D."); return null; }

        AudioClip clip = null;
        if (e.Clips != null) for (int i = 0; i < e.Clips.Length; i++) if (e.Clips[i]) { clip = e.Clips[i]; break; }
        if (!clip) return null;

        float start = (clip.length > 0f) ? Mathf.Repeat(offsetSec, clip.length) : 0f;

        var p2d = ObjectPoolManager._Inst?.Rent(_SFX2DPlayerPrefab);
        if (!p2d) return null;
        p2d.ConfigureMixer(_SFXGroup);
        p2d.Play(clip, e.Volume, true, start);
        return p2d;
    }

    public void PlayLocalByKey(string key)
        => PlayLocalByKey(key, Vector3.zero);


    // ===== 전역 재생: RPC (key만 전파) =====
    // set은 로컬 유효성 체크용(옵션). 전파에는 key만 사용.
    public void PlayLocalByKey(string key, Vector3 worldPos)
    {
        if (_SFXBlock || string.IsNullOrEmpty(key)) return;

        SoundSO.Entry e = null;
        if (_SoundDatas != null)
        {
            for (int i = 0; i < _SoundDatas.Length; i++)
            {
                var so = _SoundDatas[i];
                if (so != null && so.TryGet(key, out e)) break;
            }
        }
        if (e == null) { Debug.LogWarning($"[Audio] key not found: {key}"); return; }

        var clip = e.PickClip(); if (!clip) return;

        if (e.Space == SoundSpace.S2D)
        {
            var p2d = ObjectPoolManager._Inst?.Rent(_SFX2DPlayerPrefab);
            if (!p2d) return;
            p2d.ConfigureMixer(_SFXGroup);
            p2d.Play(clip, e.Volume, e.Loop);
        }
        else
        {
            var p3d = ObjectPoolManager._Inst?.Rent(_SFX3DPlayerPrefab);
            if (!p3d) return;
            p3d.ConfigureMixer(_SFXGroup);
            p3d.PlayAt(worldPos, clip, e.Volume, e.MinDistance, e.MaxDistance, e.Rolloff, e.Loop);
        }
    }


    public string CaptureJson()
    {
        var dto = new AudioSettingsDTO { Master = _MasterValue, BGM = _BGMValue, SFX = _SFXValue };
        return JsonUtility.ToJson(dto);
    }

    public void ApplyJson(string s)
    {
        if (string.IsNullOrEmpty(s)) return;
        var dto = JsonUtility.FromJson<AudioSettingsDTO>(s);

        _MasterValue = Mathf.Clamp01(dto.Master);
        _BGMValue = Mathf.Clamp01(dto.BGM);
        _SFXValue = Mathf.Clamp01(dto.SFX);

        if (Mixer)
        {
            Mixer.SetFloat(P_Master, ToDB(_MasterValue));
            Mixer.SetFloat(P_BGM, ToDB(_BGMValue));
            Mixer.SetFloat(P_SFX, ToDB(_SFXValue));
        }
    }
    

}
