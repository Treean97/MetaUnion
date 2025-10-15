using UnityEngine;

[CreateAssetMenu(fileName = "InfoData")]
public class InfoDataSO : ScriptableObject
{
    [Header("아이템 이름")]
    public string DisplayName;

    [Header("아이템 설명")]
    [TextArea] public string Description;

}
