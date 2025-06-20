using TMPro;
using UnityEngine;

public class FocusUIManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _Name;
    [SerializeField]
    private TMP_Text _Description;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Show(ObjectInfo objInfo)
    {
        _Name.text = objInfo.DisplayName;
        _Description.text = objInfo.Description;
    }
}
