using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class Pooled2DAudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource _AudioSource;
    Coroutine _CO;

    void Reset()
    {
        _AudioSource = GetComponent<AudioSource>();
        _AudioSource.playOnAwake = false;
        _AudioSource.loop = false;
        _AudioSource.spatialBlend = 0f; // 2D
    }

    public void SetPitch(float v)
    {
        if (_AudioSource) _AudioSource.pitch = Mathf.Clamp(v, 0.1f, 3f);
    }
    public void SetVolume(float v)
    {
        if (_AudioSource) _AudioSource.volume = Mathf.Clamp01(v);
    }

    public void ConfigureMixer(AudioMixerGroup group)
    {
        if (_AudioSource) _AudioSource.outputAudioMixerGroup = group;
    }

    public void Play(AudioClip clip, float volume = 1f, bool loop = false)
    {
        if (!clip) return;
        StopRunning();

        var camPos = Camera.main ? Camera.main.transform.position : Vector3.zero;
        transform.SetParent(null, true);
        transform.position = camPos;

        _AudioSource.spatialBlend = 0f;
        _AudioSource.loop = loop;
        _AudioSource.clip = clip;
        _AudioSource.volume = volume;
        _AudioSource.pitch = 1f; // 피치 고정
        _AudioSource.Play();

        if (!loop) _CO = StartCoroutine(CoReturnAfterRealtime(_AudioSource.clip.length));
    }

    public void Play(AudioClip clip, float volume, bool loop, float startTimeSec)
    {
        if (!clip) return;
        StopRunning();

        var camPos = Camera.main ? Camera.main.transform.position : Vector3.zero;
        transform.SetParent(null, true);
        transform.position = camPos;

        _AudioSource.spatialBlend = 0f;
        _AudioSource.loop = loop;
        _AudioSource.clip = clip;
        _AudioSource.volume = Mathf.Clamp01(volume);
        _AudioSource.pitch = 1f;
        _AudioSource.time = Mathf.Clamp(startTimeSec, 0f, Mathf.Max(0f, clip.length - 0.01f));
        _AudioSource.Play();

        if (!loop) _CO = StartCoroutine(CoReturnAfterRealtime(_AudioSource.clip.length - _AudioSource.time));    
    }

    public void StopAndReturn()
    {
        StopRunning();
        _AudioSource.Stop();
        ObjectPoolManager._Inst.Return(gameObject);
    }

    void StopRunning() { if (_CO != null) { StopCoroutine(_CO); _CO = null; } }

    IEnumerator CoReturnAfterRealtime(float t)
    {
        float end = Time.unscaledTime + t;
        while (Time.unscaledTime < end) yield return null;
        ObjectPoolManager._Inst.Return(gameObject);
    }
}
