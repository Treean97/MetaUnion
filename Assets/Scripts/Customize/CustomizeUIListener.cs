using UnityEngine;

public class CustomizeUIListener : MonoBehaviour
{
    [SerializeField] CustomizeUIManager _CustomizeUI;

    void Awake()
    {
        GameEvents.OnRequestOpenCustomizeUI += HandleRequestOpenCustomUI;
    }

    void OnDestroy()
    {
        GameEvents.OnRequestOpenCustomizeUI -= HandleRequestOpenCustomUI;
    }

    void HandleRequestOpenCustomUI()
    {
        _CustomizeUI.gameObject.SetActive(true);
    } 


}
