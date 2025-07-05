using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeItemSlot : MonoBehaviour
{
    [SerializeField] TMP_Text _Name;
    [SerializeField] Button _Btn;
    private CustomizeItemSO _ItemSO;
    

    void Awake()
    {
        // 버튼 리스너 초기화 (중복 등록 방지)
        _Btn.onClick.RemoveAllListeners();
        _Btn.onClick.AddListener(OnClick);
    }

    public void Setup(CustomizeItemSO itemSO)
    {
        _ItemSO = itemSO;
        _Name.text = itemSO.ID;
    }

    private void OnClick()
    {
        GameEvents.RaiseRequestEquipItem(_ItemSO);
    }
}
