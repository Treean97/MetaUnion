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

    void HandleSetActive(ItemDataSO itemData)
    {
        _SetAmountUIManager.SetUI(itemData);
        _SetAmountUIManager.gameObject.SetActive(true);
    }
}
