using Photon.Pun;
using TMPro;
using UnityEngine;

public class FocusUIManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _Name;
    [SerializeField]
    private TMP_Text _Description;

    public void Show(ObjectInfoSO objInfo)
    {       
        _Name.text = objInfo.DisplayName;
        _Description.text = objInfo.Description;
    }    

}
