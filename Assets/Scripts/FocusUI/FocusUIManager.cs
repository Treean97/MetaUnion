using Photon.Pun;
using TMPro;
using UnityEngine;

public class FocusUIManager : MonoBehaviourPun
{
    [SerializeField]
    private TMP_Text _Name;
    [SerializeField]
    private TMP_Text _Description;

    void Awake()
    {
        // Focus UI
        GameEvents.OnFocus += HandleFocus;
        GameEvents.OnDefocus += HandleDefocus;

        if (photonView.IsMine)
        {
            gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        // Focus UI
        GameEvents.OnFocus -= HandleFocus;
        GameEvents.OnDefocus -= HandleDefocus;
    }

    public void Show(ObjectInfo objInfo)
    {       
        _Name.text = objInfo.DisplayName;
        _Description.text = objInfo.Description;
    }
    
    private void HandleFocus(ObjectInfo objInfo)
    {
        if (!photonView.IsMine) return;
                
        Show(objInfo);
        gameObject.SetActive(true);
    }

    private void HandleDefocus()
    {
        if (!photonView.IsMine) return;

        gameObject.SetActive(false);
    }
}
