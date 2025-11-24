using Photon.Pun;
using UnityEngine;

[CreateAssetMenu(fileName = "ChangeNameAction", menuName = "Item/Actions/ChangeNameAction")]
public class ChangeNameActionSO : ScriptableObject, IInventoryAction
{
    [SerializeField] private string _Label = "사용";

    public string Label => _Label;

    public void Execute(InventoryItem inventoryItem, GameObject user)
    {
        var currentName = PlayerPrefs.GetString(PlayerPrefKeys.NicknameKey, PhotonNetwork.NickName);

        UIRouter._Inst.Open<IChangeNameUI>(ui =>
        {
            ui.SetUI(currentName, inventoryItem, user);
        });
    }
}
