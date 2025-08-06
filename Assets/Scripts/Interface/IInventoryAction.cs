using UnityEngine;

public interface IInventoryAction
{
    /// <summary>
    /// 메뉴에 표시될 라벨 (예: "사용하기", "장착하기")
    /// </summary>
    string Label { get; }

    /// <summary>
    /// 액션 수행 로직
    /// </summary>
    /// <param name="inventoryItem">클릭된 아이템 인스턴스</param>
    /// <param name="user">아이템을 사용하는 주체(플레이어 등)</param>
    void Execute(InventoryItem inventoryItem, GameObject user);
}
