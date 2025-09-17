using System.Collections.Generic;
using UnityEngine;

public enum SoundSpace { S2D, S3D }

[CreateAssetMenu(menuName = "Sound/SoundData")]
public class SoundSO : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        [Tooltip("사운드 식별자 예: ui.click, combat.slash")]
        public string Key;

        [Header("Clips")]
        public AudioClip[] Clips;

        [Header("Playback")]
        public SoundSpace Space = SoundSpace.S2D;
        [Range(0f, 2f)] public float Volume = 1f;
        public bool Loop = false;

        [Header("3D Only")]
        public float MinDistance = 1.5f;
        public float MaxDistance = 18f;
        public AudioRolloffMode Rolloff = AudioRolloffMode.Linear;

        // 랜덤 클립 추출
        public AudioClip PickClip()
            => (Clips != null && Clips.Length > 0) ? Clips[Random.Range(0, Clips.Length)] : null;
    }

    [SerializeField] private Entry[] _Items;
    private Dictionary<string, Entry> _Map;

    void OnEnable()
    {
        _Map = new Dictionary<string, Entry>(_Items?.Length ?? 0);
        if (_Items == null) return;
        foreach (var e in _Items)
        {
            var key = e.Key ?? string.Empty;
            if (!_Map.ContainsKey(key)) _Map.Add(key, e);
        }
    }

    public bool TryGet(string key, out Entry e)
    {
        if (_Map == null) OnEnable();
        if (_Map != null) return _Map.TryGetValue(key ?? string.Empty, out e);
        e = null; return false;
    }
}
