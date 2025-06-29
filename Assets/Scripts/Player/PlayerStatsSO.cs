using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "ScriptableObject/PlayerStatsSO")]
public class PlayerStatsSO : ScriptableObject
{
    [Serializable]
    public struct StatEntry
    {
        public StatType Type;
        public float    BaseValue;
    }

    [Tooltip("플레이어의 기본 스탯 리스트")]
    public List<StatEntry> DefaultStats = new List<StatEntry>();
}