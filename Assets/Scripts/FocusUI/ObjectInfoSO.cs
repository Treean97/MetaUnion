using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Object Info")]
public class ObjectInfoSO : ScriptableObject
{
    public string DisplayName;
    [TextArea] public string Description;
}
