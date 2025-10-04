using UnityEngine;

[RequireComponent(typeof(NameplateSpawner))]
public class NPCNameplate : MonoBehaviour, INameplate, INameplateVisibility
{
    private NPCSO _NPCSO;
    
    public string GetDisplayName()
    {
        _NPCSO = GetComponent<NPC>().NPCSO;

        if (_NPCSO && !string.IsNullOrEmpty(_NPCSO.DisplayName))
        {
            return _NPCSO.DisplayName;
        }

        return gameObject.name;
    }

    public bool HideForLocal() => false;
}
