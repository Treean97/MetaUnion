using UnityEngine;

[CreateAssetMenu(menuName = "WeaponState/WeaponState")]
public class WeaponStateSO : ScriptableObject
{
    public string AniTriggerName;           // "HandAttackTrigger" 등
    public DamageTool Tool;                 // Hand/Axe/Pickaxe
    public StatType DamageStat;             // AttackPower/AxePower/PickaxePower
    public Vector3 AttackOffset;            // 전방 공격 위치
    public float Radius;                    // 공격 범위
    public bool ApplyStatus;                // 상태 이상 가능
    public StatusType StatusType;           // 상태 이상 타입
    public float StatusDuration;            // 상태 이상 지속 시간
    // public AudioClip SFX;                // 타격 SFX 
}