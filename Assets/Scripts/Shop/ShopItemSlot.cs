using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상점 내 아이템 슬롯을 담당하는 컴포넌트입니다.
/// </summary>
public class ShopItemSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text _NameText;
    [SerializeField] private TMP_Text _PriceText;
    [SerializeField] private Button _Btn;

    private CustomizeItemSO _ItemSO;

    void Awake()
    {
        // 버튼 리스너 초기화 (중복 등록 방지)
        _Btn.onClick.RemoveAllListeners();
        _Btn.onClick.AddListener(OnPurchaseClicked);
    }

    /// <summary>
    /// 슬롯을 해당 아이템 정보로 설정합니다.
    /// </summary>
    public void Setup(CustomizeItemSO itemSO)
    {
        _ItemSO = itemSO;
        _NameText.text  = itemSO.ID;
        _PriceText.text = itemSO.Price.ToString();
        _Btn.interactable = true; // 재화 부족 시 외부에서 조정 가능
    }

    /// <summary>
    /// 구매 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnPurchaseClicked()
    {
        if (_ItemSO == null) return;
        GameEvents.RaiseRequestUnlockItem(_ItemSO);
    }
}
