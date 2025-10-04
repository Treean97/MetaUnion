using UnityEngine;

[CreateAssetMenu (menuName = "NPCData")]
public class NPCSO : ScriptableObject
{
    [Header("식별/표시")]
    [SerializeField] private string _NPCID;
    public string NPCID => _NPCID;
    [SerializeField] private string _DisplayName;
    public string DisplayName => _DisplayName;
    [SerializeField] private Sprite _Icon;
    public Sprite Icon => _Icon;

    [Header("대사 번들")]
    [SerializeField] DialogueSO _Dialogues;
    public DialogueSO Dialogues => _Dialogues;
}