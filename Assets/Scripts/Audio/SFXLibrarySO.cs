using System.Collections.Generic;
using UnityEngine;

public enum SFXKey : int
{

}

public class SFXLibrarySO : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public SFXKey Key;
        public AudioClip[] Clips;
        public AudioClip PickClip()
            => (Clips != null && Clips.Length > 0) ? Clips[Random.Range(0, Clips.Length)] : null;
    }

    [SerializeField] private Entry[] _Items;
    private Dictionary<SFXKey, Entry> _map;

    void OnEnable()
    {
        _map = new Dictionary<SFXKey, Entry>(_Items != null ? _Items.Length : 0);
        if (_Items != null)
            foreach (var e in _Items) if (!_map.ContainsKey(e.Key)) _map.Add(e.Key, e);
    }

    public bool TryGet(SFXKey key, out Entry e)
    {
        if (_map == null) OnEnable();

        if (_map != null)
            return _map.TryGetValue(key, out e);

        e = null;
        return false;
    }

}
