using UnityEngine;

[CreateAssetMenu (menuName = "DialogueData")]
public class DialogueSO : ScriptableObject
{
    [SerializeField] string[] _Dialogues;
    public string[] Dialogues => _Dialogues;
}
