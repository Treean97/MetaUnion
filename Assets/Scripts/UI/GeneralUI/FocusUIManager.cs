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
        if (photonView.IsMine)
        {
            gameObject.SetActive(false);    
        }
        
    }

    public void Show(ObjectInfo objInfo)
    {
        if (!photonView.IsMine) return;
        
        _Name.text = objInfo.DisplayName;
        _Description.text = objInfo.Description;
    }
}
