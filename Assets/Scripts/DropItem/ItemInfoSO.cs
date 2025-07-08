using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Object Info")]
public class ItemInfoSO : ScriptableObject
{
    public string DisplayName;
    [TextArea] public string Description;
}
