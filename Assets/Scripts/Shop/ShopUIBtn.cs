using UnityEngine;
using UnityEngine.UI;


public class ShopUIBtn : MonoBehaviour
{
    [SerializeField] Button _ShopUIBtn;

    void Awake()
    {
        _ShopUIBtn.onClick.AddListener(() => OnClickShopUIButton());
    }

    void OnClickShopUIButton()
    {
        UIRouter._Inst.Open<IShopUI>();
    }
}
