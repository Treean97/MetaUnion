using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager _Inst { get; private set; }

    [SerializeField] private AudioMixerGroup _Master;
    [SerializeField] private AudioMixerGroup _BGMGroup;
    [SerializeField] private AudioMixerGroup _SFXGroup;

    
    private AudioSource _BGMSource;   // BGM 전용(루프)
    private AudioSource _SFXSource;   // SFX 전용(PlayOneShot)

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
    }

    public void PlayBGM(AudioClip clip, bool restartIfSame = false)
    {
        if (!clip) return;

        if (!restartIfSame && _BGMSource.isPlaying && _BGMSource.clip == clip)
            return;

        _BGMSource.clip = clip;
        _BGMSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (!clip) return;
        _SFXSource.PlayOneShot(clip);
    }

}
