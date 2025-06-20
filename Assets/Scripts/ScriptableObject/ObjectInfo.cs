using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Object Info")]
public class ObjectInfo : ScriptableObject
{
    public string DisplayName;
    [TextArea] public string Description;
}
