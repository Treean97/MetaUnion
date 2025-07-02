using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public int _Gold { get; private set; }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Gold+100");
            GameEvents.RaiseRequestAddCurrency(100);
        }
    }

    void OnEnable()
    {
        GameEvents.OnRequestAddCurrency += AddGold;
        GameEvents.OnRequestSpendCurrency += SpendGold;
    }

    void OnDisable()
    {
        GameEvents.OnRequestAddCurrency -= AddGold;
        GameEvents.OnRequestSpendCurrency -= SpendGold;
    }



    void AddGold(int amount)
    {
        _Gold += amount;
        GameEvents.RaiseChangeCurrency(_Gold);
    }

    bool SpendGold(int amount)
    {
        if (_Gold < amount) return false;

        _Gold -= amount;
        GameEvents.RaiseChangeCurrency(_Gold);
        return true;
    }

}
