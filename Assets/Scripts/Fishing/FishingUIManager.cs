using System;
using UnityEngine;
using UnityEngine.UI;

public class FishingUIManager : MonoBehaviour
{
    /*
        네모 오브젝트를 좌우로 이동시켜서 네모에 닿으면 게이지 증가 아니면 감소
        네모 객체 만들어서 
        물고기 닿았다 > 이벤트 호출
        물고기 나갔다 > 이벤트 호출 이런식으로 매니저에서 게이지 관리
        
    */

    [SerializeField] private Slider _Gauge;
    [Tooltip("게이지 속도")]
    [SerializeField] private float _GaugePower;
    [Tooltip("게이지 초기 값")]
    [SerializeField] private float _GaugeSet;
    [SerializeField] private ItemDataPoolSO _RewardItemPool;
    [SerializeField] private int _MaxRewardAmount;


    public static event Action OnFishInCheckBox;
    public static void RaiseFishInCheckBox() => OnFishInCheckBox?.Invoke();
    public static event Action OnFishOutCheckBox;
    public static void RaiseFishOutCheckBox() => OnFishOutCheckBox?.Invoke();

    public static event Action OnFishingSuccess;
    public static event Action OnFishingFail;

    private bool _IsInCheckBox = false;

    void Awake()
    {
        OnFishInCheckBox += () => _IsInCheckBox = true;
        OnFishOutCheckBox += () => _IsInCheckBox = false;

        // 게이지 초기화
        _Gauge.value = _GaugeSet;
    }


    void Update()
    {
        if (_IsInCheckBox)
        {
            GaugeIncrease();
        }
        else
        {
            GaugeDecrease();
        }
    }


    // 게이지 증가
    void GaugeIncrease()
    {
        _Gauge.value += Time.deltaTime * _GaugePower;
        CheckGauge();
    }

    // 게이지 감소
    void GaugeDecrease()
    {
        _Gauge.value -= Time.deltaTime * _GaugePower;
        CheckGauge();
    }

    void CheckGauge()
    {
        if (_Gauge.value == 100)
        {
            FishingSuccess();
        }
        else if (_Gauge.value == 0)
        {
            FishingFail();
        }
    }

    void FishingSuccess()
    {
        OnFishingSuccess?.Invoke();

        // 물고기 아이템 획득 및 종료
        int idx = UnityEngine.Random.Range(0, _RewardItemPool.GetItemCount());
        var randomItem = _RewardItemPool.GetItemAt(idx);
        int amount = UnityEngine.Random.Range(1, _MaxRewardAmount);
        GameEvents.RaiseRequestItemGain(randomItem.ID, amount);

        // 낚시 종료
        FishingUIClose();
    }

    void FishingFail()
    {
        OnFishingFail?.Invoke();
        // 낚시 종료
        FishingUIClose();
    }

    void FishingUIClose()
    {
        InputBlock.UnblockInput();
        gameObject.SetActive(false);
    }
}
