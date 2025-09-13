using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager _Inst { get; private set; }

    [SerializeField] private AudioMixerGroup _Master;
    [SerializeField] private AudioMixerGroup _BGMGroup;
    [SerializeField] private AudioMixerGroup _SFXGroup;
    [SerializeField] private UISoundSO _UISound;


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

}
