using UnityEngine;
using UnityEngine.UI;


public class ShopUIBtn : MonoBehaviour
{
    [SerializeField] Button _ShopUIBtn;
    [SerializeField] GameObject _ShopUI;

    void Awake()
    {
        _ShopUIBtn.onClick.AddListener(() => OnClickShopUIButton());
    }

    void OnClickShopUIButton()
    {
        _ShopUI.gameObject.SetActive(true);
    }
}
