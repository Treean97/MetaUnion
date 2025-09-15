using System.Collections.Generic;
using UnityEngine;

public class InventoryItem
{
    public int ID;
    public int Amount;
}



public class PlayerInventory : MonoBehaviour, ISaveSection
{
    // id, count
    private InventoryItem[] _Inventory;
    [SerializeField] private int _MaxInventorySlot;

    public string Key => "inventory";

    [System.Serializable]
    private class InventoryDTO
    {
        [System.Serializable]
        public class Entry { public int id; public int amount; }

        public int slotCount;          // 저장 당시 슬롯 수(참고용)
        public List<Entry> items = new();
    }

    void Awake()
    {
        _Inventory = new InventoryItem[_MaxInventorySlot];

        for (int i = 0; i < _MaxInventorySlot; i++)
            _Inventory[i] = new InventoryItem { ID = -1, Amount = 0 };

    }

    void Start()
    {
        SaveLoadManager._Inst?.Register(this);
    }

    void OnEnable()
    {
        GameEvents.OnRequestItemGain += HandleItemGain;
        GameEvents.OnRequestItemSpend += HandleItemSpend;
        GameEvents.OnRequestInventorySlotCount += HandleInventorySlotCount;
        GameEvents.OnRequestInventoryStatus += HandleInventoryStatus;
        GameEvents.OnRequestSwapSlot += HandleSwapSlot;
        GameEvents.OnRequestCheckItemAmount += HandleCheckItemAmount;
    }

    void OnDisable()
    {
        GameEvents.OnRequestItemGain -= HandleItemGain;
        GameEvents.OnRequestItemSpend -= HandleItemSpend;
        GameEvents.OnRequestInventorySlotCount -= HandleInventorySlotCount;
        GameEvents.OnRequestInventoryStatus -= HandleInventoryStatus;
        GameEvents.OnRequestSwapSlot -= HandleSwapSlot;
        GameEvents.OnRequestCheckItemAmount -= HandleCheckItemAmount;
    }

    int HandleInventorySlotCount()
    {
        return _MaxInventorySlot;
    }

    InventoryItem[] HandleInventoryStatus()
    {
        return _Inventory;
    }

    bool HandleItemGain(int id, int amount)
    {
        Debug.Log($"Item Gain {id}, {amount}");

        // 이미 있는 슬롯에 합산
        for (int i = 0; i < _Inventory.Length; i++)
        {
            if (_Inventory[i].ID == id)
            {
                _Inventory[i].Amount += amount;
                SaveLoadManager._Inst?.RequestSaveSection(Key);
                return true;
            }
        }
        // 빈 슬롯에 새로 추가
        for (int i = 0; i < _Inventory.Length; i++)
        {
            if (_Inventory[i].ID < 0)
            {
                _Inventory[i].ID = id;
                _Inventory[i].Amount = amount;
                SaveLoadManager._Inst?.RequestSaveSection(Key);
                return true;
            }
        }
        // 슬롯 가득 찬 경우 실패
        return false;
    }

    bool HandleItemSpend(int id, int amount)
    {
        // 해당 아이템 슬롯 찾기
        for (int i = 0; i < _Inventory.Length; i++)
        {
            if (_Inventory[i].ID == id && _Inventory[i].Amount >= amount)
            {
                _Inventory[i].Amount -= amount;

                // 슬롯이 비었으면 ID 초기화
                if (_Inventory[i].Amount == 0)
                {
                    _Inventory[i].ID = -1;
                }

                SaveLoadManager._Inst?.RequestSaveSection(Key);
                return true;
            }
        }
        return false;
    }


    void HandleSwapSlot(int from, int to)
    {
        // 같은 슬롯일 경우
        if (from == to)
        {
            return;
        }

        // 인덱스 교환
        (_Inventory[from], _Inventory[to]) = (_Inventory[to], _Inventory[from]);

        GameEvents.RaiseRequestUpdateInventory();
        SaveLoadManager._Inst?.RequestSaveSection(Key);
    }

    bool HandleCheckItemAmount(int id, int amount)
    {
        // 추후 스택 제한을 넣는다는 가정
        int total = 0;

        for (int i = 0; i < _Inventory.Length; i++)
        {
            if (_Inventory[i].ID != id) continue;

            total += _Inventory[i].Amount;

            if (total >= amount)
            {
                Debug.Log("Item is enough");
                return true;
            }

        }

        // 인벤토리 비었음
        return false;
    }

    public string CaptureJson()
    {
        var dto = new InventoryDTO { slotCount = _Inventory.Length };

        for (int i = 0; i < _Inventory.Length; i++)
        {
            // 비어있음은 id = -1, amount = 0으로 통일
            int id = _Inventory[i].ID >= 0 ? _Inventory[i].ID : -1;
            int amt = _Inventory[i].Amount > 0 ? _Inventory[i].Amount : 0;

            dto.items.Add(new InventoryDTO.Entry { id = id, amount = amt });
        }

        return JsonUtility.ToJson(dto);    
    }

    public void ApplyJson(string s)
    {
        if (string.IsNullOrEmpty(s)) return;

        InventoryDTO dto = null;
        try { dto = JsonUtility.FromJson<InventoryDTO>(s); } catch { }
        if (dto == null || dto.items == null) return;

        // 저장 당시 슬롯 수와 현재 _MaxInventorySlot이 다를 수 있으므로
        // 현재 크기(_MaxInventorySlot)를 기준으로 안전하게 채움
        int n = _MaxInventorySlot;

        // 내부 배열 크기 보정
        if (_Inventory == null || _Inventory.Length != n)
        {
            _Inventory = new InventoryItem[n];
            for (int i = 0; i < n; i++) _Inventory[i] = new InventoryItem { ID = -1, Amount = 0 };
        }

        for (int i = 0; i < n; i++)
        {
            if (i < dto.items.Count)
            {
                var e = dto.items[i];
                int id = e.id >= 0 ? e.id : -1;
                int amt = e.amount > 0 ? e.amount : 0;

                _Inventory[i].ID = (amt > 0) ? id : -1; // 수량 0이면 빈 슬롯
                _Inventory[i].Amount = (amt > 0) ? amt : 0;
            }
            else
            {
                _Inventory[i].ID = -1;
                _Inventory[i].Amount = 0;
            }
        }

        // UI 갱신 필요 시
        GameEvents.RaiseRequestUpdateInventory();    
    }
}
