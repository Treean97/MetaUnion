using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeItemSlot : MonoBehaviour, IItemDataProvider
{
    [SerializeField] TMP_Text _Name;
    [SerializeField] Image _Icon;
    [SerializeField] Button _EquipButton;
    [SerializeField] Button _ColorButton;
    private TMP_Text _ButtonText;
    private CustomizeItemSO _ItemSO;

    // UI 갱신용 접근자
    public ItemType Type => _ItemSO.Type;
    public string   ID   => _ItemSO.ID;


    void Awake()
    {
        // 버튼 리스너 초기화 (중복 등록 방지)
        _EquipButton.onClick.RemoveAllListeners();
        _EquipButton.onClick.AddListener(OnClickButton);

        _ColorButton.onClick.RemoveAllListeners();
        _ColorButton.onClick.AddListener(OnClickColorButton);

        _ButtonText = _EquipButton.GetComponentInChildren<TMP_Text>();
    }

    public void Setup(CustomizeItemSO itemSO)
    {
        _ItemSO = itemSO;
        _Icon.sprite = itemSO.Sprite;
        _Name.text = itemSO.InfoDataSO.DisplayName;
    }

    private void OnClickButton()
    {
        GameEvents.RaiseRequestEquipItem(_ItemSO);
    }

    void OnClickColorButton()
    {
        UIRouter._Inst.Open<IColorUI>(ui => ui.SetUI(_ItemSO));
    }

    public void SetState(bool equipped)
    {
        _ButtonText.text = equipped ? "해제" : "착용";
    }

    public InfoDataSO GetItemData()
    {
        return _ItemSO.InfoDataSO;
    }
}
