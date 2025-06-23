using UnityEngine;

public class CustomizeUIListener : MonoBehaviour
{
    [SerializeField] CustomizeUIManager _CustomizeUI;

    void OnEnable()
    {
        GameEvents.OnRequestOpenCustomizeUI += HandleRequestOpenCustomUI;
    }

    void OnDisable()
    {
        GameEvents.OnRequestOpenCustomizeUI -= HandleRequestOpenCustomUI;
    }

    void HandleRequestOpenCustomUI()
    {

        _CustomizeUI.gameObject.SetActive(true);
    } 


}
