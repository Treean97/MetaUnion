using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardUI : MonoBehaviour
{
    [SerializeField] GameObject _RewardUI;
    [SerializeField] Image _RewardIcon;
    [SerializeField] TMP_Text _RewardAmount;
    [SerializeField] RewardEffect _RewardEffect;
    [SerializeField] float _NoticeTime = 3f;
    [SerializeField] string _RewardSuccessKey = "Reward_Success";
    [SerializeField] string _RewardFailKey = "Reward_Fail";

    Coroutine _CO;

    void Start()
    {
        GameEvents.OnRewardSuccess += HandleRewardSuccess;
        GameEvents.OnRewardFail += HandleRewardFail;
    }

    void OnDestroy()
    {
        GameEvents.OnRewardSuccess -= HandleRewardSuccess;
        GameEvents.OnRewardFail -= HandleRewardFail;
        if (_CO != null) { StopCoroutine(_CO); _CO = null; }
    }

    void HandleRewardSuccess(RewardType type, int itemId, int amount)
    {
        if (_CO != null) StopCoroutine(_CO);
        _CO = StartCoroutine(RewardSuccessCO(type, itemId, amount));       
    }

    IEnumerator RewardSuccessCO(RewardType type, int id, int amount)
    {
        ItemDataSO itemData = null;

        switch(type)
        {
            case RewardType.Item:
                ItemManager._Inst.ItemDataPoolSO.TryGetItem(id, out itemData);
                break;
            case RewardType.Currency:
                CurrencyManager._Inst.CurrencyPoolSO.TryGetCurrency(id, out itemData);
                break;
        }        

        _RewardIcon.sprite = itemData.Icon;
        _RewardAmount.text = amount.ToString();
        UIFX.Show(_RewardUI);
        _RewardEffect.Play();

        AudioManager._Inst.PlayLocalByKey(_RewardSuccessKey);
        yield return new WaitForSecondsRealtime(_NoticeTime);

        UIFX.Hide(_RewardUI);
        _CO = null;
    }
    
    void HandleRewardFail()
    {
        AudioManager._Inst.PlayLocalByKey(_RewardFailKey);
    }
}
