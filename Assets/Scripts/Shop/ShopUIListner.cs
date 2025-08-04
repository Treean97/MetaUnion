using UnityEngine;

public class ShopUiListner : MonoBehaviour
{
    [SerializeField] ShopUIManager _ShopUI;

    void Awake()
    {
        GameEvents.OnRequestOpenShopUI += HandleRequestOpenCustomUI;
    }

    void OnDestroy()
    {
        GameEvents.OnRequestOpenShopUI -= HandleRequestOpenCustomUI;
    }

    void HandleRequestOpenCustomUI()
    {
        _ShopUI.gameObject.SetActive(true);
    } 


}
