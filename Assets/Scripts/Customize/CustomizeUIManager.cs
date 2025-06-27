using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeUIManager : MonoBehaviour
{
    [SerializeField] CustomizeItemPoolSO _CustomizeItemPool;
    [SerializeField] Transform _Contents;
    [SerializeField] GameObject _CustomizeItemSlotPrefab;
    [SerializeField] List<Button> _CategoryBtns;
    [SerializeField] Button _UICloseBtn;

    private ItemType _CurType;

    void Awake()
    {
        //CustomizeItemPoolLocator.Register(_CustomizeItemPool);

        _UICloseBtn.onClick.AddListener(OnClickUICloseBtn);
    }

    // 처음에 최상단 카테고리(Hair)로 초기화 해놓고 그 뒤로는 켤 때 기존 상태대로 보여주면 될 듯
    // 아니면 현재 카테고리 변수를 하나 저장해놓던가
    void Start()
    {
        for (int i = 0; i < _CategoryBtns.Count; i++)
        {
            int idx = i;
            _CategoryBtns[i].onClick.AddListener(() => ChangeCategory((ItemType)idx));

        }

        _CurType = _CustomizeItemPool.GetFirstType();
        UpdateItemList(_CurType);

        // 꺼두기
        gameObject.SetActive(false);
    }

    public void ChangeCategory(ItemType newType)
    {
        if (_CurType == newType) return;
        _CurType = newType;
        UpdateItemList(_CurType);
    }

    void OnEnable()
    {
        GameEvents.OnRequestEquipItem += HandleEquipItem;
    }

    void OnDisable()
    {
        GameEvents.OnRequestEquipItem -= HandleEquipItem;
    }

    private void HandleEquipItem(CustomizeItemSO item)
    {
        var player = PlayerSetup._LocalPlayer.GetComponent<PlayerCustomization>();
        player?.EquipItem(item);
    }

    void UpdateItemList(ItemType type)
    {
        // 기존 슬롯 제거
        foreach (Transform child in _Contents)
            Destroy(child.gameObject);

        // 풀에서 해당 타입 아이템 리스트 획득
        IReadOnlyList<CustomizeItemSO> items = _CustomizeItemPool.GetItems(type);

        // 슬롯 생성
        foreach (var item in items)
        {
            GameObject slot = Instantiate(_CustomizeItemSlotPrefab, _Contents);
            slot.GetComponent<CustomizeItemSlot>().Setup(item);
        }
    }

    void OnClickUICloseBtn()
    {
        gameObject.SetActive(false);
    }
}
