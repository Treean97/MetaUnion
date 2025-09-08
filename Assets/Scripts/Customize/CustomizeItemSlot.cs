using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeItemSlot : MonoBehaviour
{
    [SerializeField] TMP_Text _Name;
    [SerializeField] Button _Button;
    private TMP_Text _ButtonText;
    private CustomizeItemSO _ItemSO;

    // UI 갱신용 접근자
    public ItemType Type => _ItemSO.Type;
    public string   ID   => _ItemSO.ID;


    void Awake()
    {
        // 버튼 리스너 초기화 (중복 등록 방지)
        _Button.onClick.RemoveAllListeners();
        _Button.onClick.AddListener(OnClickButton);

        _ButtonText = _Button.GetComponentInChildren<TMP_Text>();
    }

    public void Setup(CustomizeItemSO itemSO)
    {
        _ItemSO = itemSO;
        _Name.text = itemSO.ID;
    }

    private void OnClickButton()
    {
        GameEvents.RaiseRequestEquipItem(_ItemSO);
    }

    public void SetState(bool equipped)
    {
        _ButtonText.text = equipped ? "해제" : "착용";
    }
}
