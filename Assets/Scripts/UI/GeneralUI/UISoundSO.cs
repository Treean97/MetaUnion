using UnityEngine;

[CreateAssetMenu(menuName = "Sound/UISoundData")]
public class UISoundSO : ScriptableObject
{
    [Header("UI Button")]
    public AudioClip[] ClickPool;   // 클릭은 여기서 랜덤
    public AudioClip   Hover;       // 호버는 단일

    [Header("UI Open/Close")]
    public AudioClip   UIPop;       // UI 열림
    public AudioClip   UIClose;     // UI 닫힘

}
