using UnityEngine;

public enum PotionEffectType
{
    SpeedUp,
    JumpUp
}

[CreateAssetMenu(fileName = "UseAction", menuName = "Item/Actions/UseAction")]
public class UseActionSO : ScriptableObject, IInventoryAction
{
    [SerializeField] private string _Label = "Use";

    public string Label => _Label;

    [SerializeField] private PotionEffectType _EffectType;
    [SerializeField] private float _Value;
    [SerializeField] private float _Duration;

    public void Execute(InventoryItem inventoryItem, GameObject user)
    {
        switch (_EffectType)
        {
            case PotionEffectType.SpeedUp:
                GameEvents.RaiseRequestMoveSpeedBuff(_Value, _Duration);
                break;

            case PotionEffectType.JumpUp:
                GameEvents.RaiseRequestJumpBoostBuff(_Value, _Duration);
                break;
        }

        GameEvents.RaiseRequestItemSpend(inventoryItem.ID, 1);
    }

}
