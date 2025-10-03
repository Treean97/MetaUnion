using UnityEngine;

[CreateAssetMenu (menuName = "NPCData")]
public class NPCSO : ScriptableObject
{
    [Header("식별/표시")]
    public string NPCID;
    public string DisPlayName;
    public Sprite Icon;

    [Header("대사 번들")]
    public DialogueSO Dialogues;
}