using System;
using System.Collections;
using UnityEngine;

public class FishingSequence : MonoBehaviour, IFishingUI, ILeftClickConsumer
{
    [SerializeField] private FishingMinigame _MinigameUI;
    [SerializeField] private GameObject _CatchUI;

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

    // 좌클릭 소비 등록 토큰
    IDisposable _LeftClickToken;

    // Catchable에서 클릭이 들어왔는지
    bool _CatchClicked;

    public bool IsOpen => gameObject.activeSelf;

    public static event Action OnFishingStart;
    public static event Action OnFishingSuccess;
    public static event Action OnFishingFail;
    public static event Action OnFishingEnd;

    void OnEnable()
    {
        FishingMinigame.OnFishingSuccess += HandleMinigameSuccess;
        FishingMinigame.OnFishingFail += HandleMinigameFail;

        // 낚시 UI(시퀀스)가 켜졌다는 건 "낚시 흐름 진행 중"이므로
        // 좌클릭을 여기서 우선 소비하도록 등록 (공격 방지)
        _LeftClickToken = LeftClickDispatcher._Inst?.Push(this);
        _CatchClicked = false;
    }

    void OnDisable()
    {
        FishingMinigame.OnFishingSuccess -= HandleMinigameSuccess;
        FishingMinigame.OnFishingFail -= HandleMinigameFail;

        _LeftClickToken?.Dispose();
        _LeftClickToken = null;
        _CatchClicked = false;
    }

    // === 좌클릭 소비 ===
    public bool ConsumeLeftClick()
    {
        // Catchable일 때만 "잡기"로 처리하고,
        // 그 외 상태에서는 공격으로 안 넘어가게 그냥 소비만 한다.
        if (_State == FishingState.Catchable)
        {
            _CatchClicked = true;
        }

        // 낚시 UI가 켜져 있고, Idle이 아니면 항상 소비해서 공격을 막는다.
        return _State != FishingState.Idle;
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
        _CatchClicked = false;
        UIFX.Show(_CatchUI);

        float time = 0f;

        while (time < _CatchableSeconds)
        {
            if (_CatchClicked)
            {
                UIFX.Hide(_CatchUI);
                break;
            }

            time += Time.deltaTime;
            yield return null;
        }

        bool catched = _CatchClicked;
        _CatchClicked = false;

        if (!catched)
        {
            // 시간 초과 실패
            UIFX.Hide(_CatchUI);
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
