using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPreviewRouter : MonoBehaviour
{
    [Serializable] public class Entry {
        public string id;           // "Idle", "Walk", "Run" 등 버튼용 키
        public string stateName;    // 애니메이터 상태 이름(또는 경로)
        public float  fade = 0.15f;
    }

    [SerializeField] Animator _Animator;
    [SerializeField] List<Entry> _Entries = new();

    readonly Dictionary<string, (int hash, float fade)> _map = new();

    void Awake()
    {
        foreach (var e in _Entries)
        {
            if (string.IsNullOrEmpty(e.id) || string.IsNullOrEmpty(e.stateName)) continue;
            _map[e.id] = (Animator.StringToHash(e.stateName), Mathf.Max(0f, e.fade));
        }

        // 애니메이션 이벤트 받지 않음
        _Animator.fireEvents = false;
    }

    public void Play(string id, int layer = 0, float normalizedTime = 0f)
    {
        if (!_Animator) return;
        if (!_map.TryGetValue(id, out var v))
        {
            Debug.LogWarning($"[PreviewPoseRouter] Unknown id: {id}");
            return;
        }
        _Animator.CrossFade(v.hash, v.fade, layer, normalizedTime);
    }
}
