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
    const string P_BGM    = "BGM";
    const string P_SFX    = "SFX";

    [SerializeField] private AudioMixerGroup _Master;
    [SerializeField] private AudioMixerGroup _BGMGroup;
    [SerializeField] private AudioMixerGroup _SFXGroup;
    [SerializeField] private UISoundSO _UISound;

    AudioMixer Mixer => _Master ? _Master.audioMixer :
                        _BGMGroup ? _BGMGroup.audioMixer :
                        _SFXGroup ? _SFXGroup.audioMixer : null;


    private AudioSource _BGMSource;   // BGM 전용(루프)
    private AudioSource _SFXSource;   // SFX 전용(PlayOneShot)

    float _MasterValue = 1f, _BGMValue = 1f, _SFXValue = 1f;

    // DB로 변환
    static float ToDB(float v) => v <= 0.0001f ? -80f : Mathf.Log10(v) * 20f;

    void Awake()
    {
        if (_Inst != null) { Destroy(gameObject); return; }
        _Inst = this;
        DontDestroyOnLoad(gameObject);

        // BGM 소스
        _BGMSource = gameObject.AddComponent<AudioSource>();
        _BGMSource.playOnAwake = false;
        _BGMSource.loop = true;
        _BGMSource.spatialBlend = 0f; // 2D
        _BGMSource.outputAudioMixerGroup = _BGMGroup;

        // SFX 소스
        _SFXSource = gameObject.AddComponent<AudioSource>();
        _SFXSource.playOnAwake = false;
        _SFXSource.loop = false;
        _SFXSource.spatialBlend = 0f; // 2D
        _SFXSource.outputAudioMixerGroup = _SFXGroup;

        SaveLoadManager._Inst?.Register(this);
    }

    public float GetMasterValue() => _MasterValue;
    public float GetBGMValue()    => _BGMValue;
    public float GetSFXValue()    => _SFXValue;

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

    public void PlaySFX(AudioClip clip)
    {
        if (!clip) return;
        _SFXSource.PlayOneShot(clip);
    }

    public void PlayUIClick()
    {
        if (!_UISound || _UISound.ClickPool == null || _UISound.ClickPool.Length == 0) return;
        var pool = _UISound.ClickPool;
        var clip = pool[Random.Range(0, pool.Length)];
        PlaySFX(clip);
    }

    public void PlayUIHover()
    {
        if (!_UISound || !_UISound.Hover) return;
        PlaySFX(_UISound.Hover);
    }

    public void PlayUIPop()
    {
        if (!_UISound || !_UISound.UIPop) return;
        PlaySFX(_UISound.UIPop);
    }

    public void PlayUIClose()
    {
        if (!_UISound || !_UISound.UIClose) return;
        PlaySFX(_UISound.UIClose);
    }

    public string CaptureJson()
    {
        var dto = new AudioSettingsDTO
        {
            Master = _MasterValue,
            BGM = _BGMValue,
            SFX = _SFXValue,
        };
        return JsonUtility.ToJson(dto);
    }

    public void ApplyJson(string s)
    {
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
