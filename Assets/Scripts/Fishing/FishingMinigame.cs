using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class FishingMinigame : MonoBehaviour
{
    [SerializeField] private Slider _Gauge;
    [Tooltip("게이지 속도")]
    [SerializeField] private float _GaugePower;
    [Tooltip("게이지 초기 값")]
    [SerializeField] private float _GaugeSet;

    private bool _IsInCheckBox = false;


    public static event Action OnFishInCheckBox;
    public static void RaiseFishInCheckBox() => OnFishInCheckBox?.Invoke();
    public static event Action OnFishOutCheckBox;
    public static void RaiseFishOutCheckBox() => OnFishOutCheckBox?.Invoke();

    public static event Action OnFishingSuccess;
    public static event Action OnFishingFail;


    void OnEnable()
    {
        OnFishInCheckBox += HandleFishIn;
        OnFishOutCheckBox += HandleFishOut;
    }

    void OnDisable()
    {
        OnFishInCheckBox -= HandleFishIn;
        OnFishOutCheckBox -= HandleFishOut;
    }

    public void OpenMinigame()
    {
        _IsInCheckBox = false;
        _Gauge.value = _GaugeSet;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!gameObject.activeSelf) return;

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
        if (_Gauge.value >= 100)
        {
            OnFishingSuccess?.Invoke();
            FishingUIClose();
        }
        else if (_Gauge.value <= 0)
        {
            OnFishingFail?.Invoke();
            FishingUIClose();
        }
    }

    void HandleFishIn()
    {
        _IsInCheckBox = true;
    }

    void HandleFishOut()
    {
        _IsInCheckBox = false;
    }   

    public void FishingUIClose()
    {
        InputBlock.UnblockInput();
        gameObject.SetActive(false);
    }
}
