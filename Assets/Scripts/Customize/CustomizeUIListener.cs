using Photon.Pun;
using UnityEngine;

public class CustomizeUIListener : MonoBehaviourPun
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
        if (!photonView.IsMine) return;

        _CustomizeUI.gameObject.SetActive(true);
    } 


}
