using TMPro;
using UnityEngine;

public class CustomizeItemSlot : MonoBehaviour
{
    [SerializeField] TMP_Text _Name;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Setup(CustomizeItemSO itemSO)
    {
        _Name.text = itemSO.ID;
    }
}
