using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class Pooled3DAudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource _AudioSource;
    Coroutine _CO;

    void Reset()
    {
        _AudioSource = GetComponent<AudioSource>();
        _AudioSource.playOnAwake = false;
        _AudioSource.loop = false;
        _AudioSource.spatialBlend = 1f;
        _AudioSource.dopplerLevel = 0f;
        _AudioSource.rolloffMode = AudioRolloffMode.Linear;
        _AudioSource.minDistance = 1.5f;
        _AudioSource.maxDistance = 18f;
    }

    public void ConfigureMixer(AudioMixerGroup group)
    {
        if (_AudioSource) _AudioSource.outputAudioMixerGroup = group;
    }

    public void PlayAt(Vector3 pos, AudioClip clip, float volume,
                       float minDist, float maxDist, AudioRolloffMode rolloff, bool loop = false)
    {
        if (!clip) return;
        StopRunning();

        transform.SetParent(null, true);
        transform.position = pos;

        _AudioSource.spatialBlend = 1f;
        _AudioSource.loop = loop;
        _AudioSource.clip = clip;
        _AudioSource.volume = volume;
        _AudioSource.pitch = 1f; // 피치 고정
        _AudioSource.rolloffMode = rolloff;
        _AudioSource.minDistance = minDist;
        _AudioSource.maxDistance = maxDist;
        _AudioSource.Play();

        if (!loop) _CO = StartCoroutine(CoReturnAfterRealtime(_AudioSource.clip.length));
    }

    public void PlayAttached(Transform target, AudioClip clip, float volume,
                             float minDist, float maxDist, AudioRolloffMode rolloff, bool loop = true)
    {
        StopRunning();

        transform.SetParent(target, false);
        transform.localPosition = Vector3.zero;

        _AudioSource.spatialBlend = 1f;
        _AudioSource.loop = loop;
        _AudioSource.clip = clip;
        _AudioSource.volume = volume;
        _AudioSource.pitch = 1f;
        _AudioSource.rolloffMode = rolloff;
        _AudioSource.minDistance = minDist;
        _AudioSource.maxDistance = maxDist;
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
