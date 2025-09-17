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
