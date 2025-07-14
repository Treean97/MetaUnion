using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DropTable/DropTable Data")]
public class DropItemTableSO : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public GameObject ItemPrefab;
        [Range(0f, 1f)] public float DropChance;
        public int MinAmount;
        public int MaxAmount;
    }

    [Tooltip("확률 합은 1.0을 초과해도 무방하며, 각 확률을 독립적으로 판정합니다.")]
    [SerializeField] private List<Entry> _Entries = new();
    public IReadOnlyList<Entry> Entries => _Entries;
}
