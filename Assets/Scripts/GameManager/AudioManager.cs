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


public class AudioManager : MonoBehaviour, ISaveSection
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

        // BGM 소스
        _BGMSource = gameObject.AddComponent<AudioSource>();
        _BGMSource.playOnAwake = false;
        _BGMSource.loop = true;
        _BGMSource.spatialBlend = 0f; // 2D
        _BGMSource.outputAudioMixerGroup = _BGMGroup;

        SaveLoadManager._Inst?.Register(this);
    }

    public float GetMasterValue() => _MasterValue;
    public float GetBGMValue() => _BGMValue;
    public float GetSFXValue() => _SFXValue;

    public void SetMasterValue(float v, bool requestSave = true)
    {
        _MasterValue = Mathf.Clamp01(v);
        if (Mixer) Mixer.SetFloat(P_Master, ToDB(_MasterValue));
        if (requestSave) SaveLoadManager._Inst?.RequestSaveSection(Key);
    }

    public void SetBGMValue(float v, bool requestSave = true)
    {
        _BGMValue = Mathf.Clamp01(v);
        if (Mixer) Mixer.SetFloat(P_BGM, ToDB(_BGMValue));
        if (requestSave) SaveLoadManager._Inst?.RequestSaveSection(Key);
    }

    public void SetSFXValue(float v, bool requestSave = true)
    {
        _SFXValue = Mathf.Clamp01(v);
        if (Mixer) Mixer.SetFloat(P_SFX, ToDB(_SFXValue));
        if (requestSave) SaveLoadManager._Inst?.RequestSaveSection(Key);
    }

    public void PlayBGM(AudioClip clip)
    {
        if (!clip) return;
        if (_BGMSource.clip == clip) return;
        _BGMSource.clip = clip;
        _BGMSource.Play();
    }

    // ===== 로컬 재생: SO + Key =====
    // 3D: pos(원샷) 또는 attach(부착) 중 하나 사용
    public Pooled3DAudioPlayer PlayLocalBySO(SoundSO soundData, string key, Vector3? pos = null, Transform attach = null)
    {
        if (_SFXBlock || !soundData || !soundData.TryGet(key, out var e)) return null;
        var clip = e.PickClip(); if (!clip) return null;

        if (e.Space == SoundSpace.S2D)
        {
            var p2d = ObjectPoolManager._Inst?.Rent(_SFX2DPlayerPrefab);
            if (!p2d) { Debug.LogWarning("[Audio] 2D pool not ready"); return null; }
            p2d.ConfigureMixer(_SFXGroup);
            p2d.Play(clip, e.Volume, e.Loop);
            return null; // 2D는 핸들 불필요
        }
        else
        {
            var p3d = ObjectPoolManager._Inst?.Rent(_SFX3DPlayerPrefab);
            if (!p3d) { Debug.LogWarning("[Audio] 3D pool not ready"); return null; }
            p3d.ConfigureMixer(_SFXGroup);

            if (attach)
                p3d.PlayAttached(attach, clip, e.Volume, e.MinDistance, e.MaxDistance, e.Rolloff, e.Loop);
            else
                p3d.PlayAt(pos ?? Vector3.zero, clip, e.Volume, e.MinDistance, e.MaxDistance, e.Rolloff, e.Loop);

            return p3d; // 루프/부착이면 나중에 StopAndReturn() 호출용
        }
    }

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
