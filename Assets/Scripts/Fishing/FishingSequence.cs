using System;
using System.Collections;
using UnityEngine;

public class FishingManager : MonoBehaviour, IFishingUI
{
    [SerializeField] private FishingMinigame _MinigameUI;
    [SerializeField] private KeyCode _CatchKey = KeyCode.Mouse0;

    [SerializeField] private ItemDataPoolSO _RewardItemPool;
    [SerializeField] private int _MaxRewardAmount = 9;

    [SerializeField] private float[] _BiteDelay = new float[] { 1f, 5f };
    [SerializeField] private float _CatchableSeconds = 1f;
    [SerializeField] private float _Cooldown = 1f;

    private enum FishingState { Idle, Casting, WaitingBite, Catchable, Minigame, Resolve }
    private FishingState _State = FishingState.Idle;
    private Coroutine _Routine;
    private bool? _MinigameResult;

    public bool IsOpen => gameObject.activeSelf;

    public static event Action OnCastStarted;
    public static event Action OnWaitLoopStarted;
    public static event Action OnFishingSucceeded;
    public static event Action OnFishingFailed;

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
        OnCastStarted?.Invoke();
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
            OnFishingFailed?.Invoke();
            yield return new WaitForSeconds(_Cooldown);
            ResetFlow();
            yield break;
        }

        // 미니게임
        _State = FishingState.Minigame;
        _MinigameResult = null;
        _MinigameUI.OpenMinigame();

        yield return new WaitUntil(() => _MinigameResult.HasValue);

        // 보상
        _State = FishingState.Resolve;

        if (_MinigameResult == true)
        {
            OnFishingSucceeded?.Invoke(); 

            int idx = UnityEngine.Random.Range(0, _RewardItemPool.GetItemCount());
            ItemDataSO randomItem = _RewardItemPool.GetItemAt(idx);
            int amount = UnityEngine.Random.Range(1, _MaxRewardAmount + 1);
            GameEvents.RaiseRequestItemGain(randomItem.ID, amount);
        }
        else
        {
            OnFishingFailed?.Invoke();
        }

        yield return new WaitForSeconds(_Cooldown);
        ResetFlow();
    }

    void HandleMinigameSuccess()
    {
        _MinigameResult = true;
    }

    void HandleMinigameFail()
    {
        _MinigameResult = false;
    }

    void ResetFlow()
    {
        if (_Routine != null) StopCoroutine(_Routine);
        _Routine = null;
        _State = FishingState.Idle;

        if (_MinigameUI.gameObject.activeSelf)
        {
            _MinigameUI.FishingUIClose();
        }
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        StartFishing();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
