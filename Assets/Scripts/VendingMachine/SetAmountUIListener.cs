using UnityEngine;

public class SetAmountUIListener : MonoBehaviour
{
    [SerializeField] SetAmountUIManager _SetAmountUIManager;

    void OnEnable()
    {
        GameEvents.OnRequestOpenSetAmountUI += HandleSetActive;
    }

    void OnDisable()
    {
        GameEvents.OnRequestOpenSetAmountUI -= HandleSetActive;
    }

    void HandleSetActive(QuantityMode mode, ItemDataSO itemData)
    {
        _SetAmountUIManager.SetUI(mode, itemData);
        _SetAmountUIManager.gameObject.SetActive(true);
    }
}
