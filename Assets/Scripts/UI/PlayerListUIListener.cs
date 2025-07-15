using UnityEngine;

public class PlayerListUIListener : MonoBehaviour
{
    [SerializeField] PlayerListUIManager _PlayerListUI;

    void OnEnable()
    {
        GameEvents.OnRequestOpenPlayerListUI += HandleOpenUI;
        GameEvents.OnRequestClosePlayerListUI += HandleCloseUI;
    }

    void OnDisable()
    {
        GameEvents.OnRequestOpenPlayerListUI -= HandleOpenUI;
        GameEvents.OnRequestClosePlayerListUI -= HandleCloseUI;
    }

    void HandleOpenUI()
    {
        _PlayerListUI.gameObject.SetActive(true);
    }

    void HandleCloseUI()
    {
        _PlayerListUI.gameObject.SetActive(false);
    }
}
