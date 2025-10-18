using System;
using System.Collections;
using UnityEngine;

public class FishingSequence : MonoBehaviour, IFishingUI
{
    
    [SerializeField] private FishingMinigame _MinigameUI;
    [SerializeField] private KeyCode _CatchKey = KeyCode.Mouse0;

    [SerializeField] private ItemDataPoolSO _RewardItemPool;
    [SerializeField] private int _MaxRewardAmount = 9;

    [SerializeField] private float[] _BiteDelay = new float[] { 3f, 5f };
    [SerializeField] private float _CatchableSeconds = 1f;
    [SerializeField] private float _Cooldown = 2f;

    [Header("Sound")]
    [SerializeField] private string _ReelKey;

    private enum FishingState { Idle, Casting, WaitingBite, Catchable, Minigame, Resolve }
    private FishingState _State = FishingState.Idle;
    private Coroutine _Routine;
    private bool? _MinigameResult;

    public bool IsOpen => gameObject.activeSelf;

    public static event Action OnFishingStart;
    public static event Action OnWaitLoopStart;
    public static event Action OnFishingSuccess;
    public static event Action OnFishingFail;
    public static event Action OnFishingEnd;

    void OnEnable()
    {
        FishingMinigame.OnFishingSuccess += HandleMinigameSuccess;
        FishingMinigame.OnFishingFail += HandleMinigameFail;
        
    }

    void OnDisable()
    {
        FishingMinigame.OnFishingSuccess -= HandleMinigameSuccess;
        FishingMinigame.OnFishingFail -= HandleMinigameFail;
        
    }

    public bool StartFishing()
    {
        if (_State != FishingState.Idle || _Routine != null) return false;
        _Routine = StartCoroutine(FishingRoutine());
        return true;
    }

    IEnumerator FishingRoutine()
    {
        // 캐스팅
        _State = FishingState.Casting;
        OnFishingStart?.Invoke();
        yield return null;

        // 대기
        _State = FishingState.WaitingBite;
        float delay = UnityEngine.Random.Range(_BiteDelay[0], _BiteDelay[1]);
        yield return new WaitForSeconds(delay);

        // 잡기 가능
        _State = FishingState.Catchable;
        GameEvents.RaiseShowWarning("Catch!!!", _CatchableSeconds);
        float time = 0f;
        bool catched = false;

        while (time < _CatchableSeconds)
        {
            if (Input.GetKeyDown(_CatchKey))
            {
                catched = true;
                break;
            }

            time += Time.deltaTime;
            yield return null;
        }

        if (!catched)
        {
            _State = FishingState.Resolve;
            OnFishingFail?.Invoke();
            yield return new WaitForSeconds(_Cooldown);
            ResetFlow();
            yield break;
        }

        // 미니게임
        _State = FishingState.Minigame;

        // 릴 사운드
        var reelSound = AudioManager._Inst.Play2DLoopLocalPlayByKey(_ReelKey);

        _MinigameResult = null;
        _MinigameUI.OpenMinigame();

        yield return new WaitUntil(() => _MinigameResult.HasValue);

        // 보상
        _State = FishingState.Resolve;

        // 릴 사운드 중단
        reelSound.StopAndReturn();

        if (_MinigameResult == true)
        {
            OnFishingSuccess?.Invoke();            
        }
        else
        {
            OnFishingFail?.Invoke();            
        }

        // 낚시 종료 전파
        OnFishingEnd?.Invoke();
        yield return new WaitForSeconds(_Cooldown);
        ResetFlow();
    }

    void HandleMinigameSuccess()
    {
        _MinigameResult = true;

        int idx = UnityEngine.Random.Range(0, _RewardItemPool.GetItemCount());
        ItemDataSO randomItem = _RewardItemPool.GetItemAt(idx);
        int amount = UnityEngine.Random.Range(1, _MaxRewardAmount + 1);
        GameEvents.RaiseRewardSuccess(RewardType.Item, randomItem.ID, amount);
    }

    void HandleMinigameFail()
    {
        _MinigameResult = false;

        GameEvents.RaiseRewardFail();
    }

    void ResetFlow()
    {        
        if (_Routine != null) StopCoroutine(_Routine);
        _Routine = null;
        _State = FishingState.Idle;        
        gameObject.SetActive(false);
    }

    public void Show() 
    {
        StartFishing();
    }

    public void Hide() { }
}
