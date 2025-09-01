using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SlotMachineUIManager : MonoBehaviour, ISlotMachineUI
{
    [SerializeField] RectTransform[] _LaneContents;
    int _Lane0_MaxSlot = 30;
    int _Lane1_MaxSlot = 40;
    int _Lane2_MaxSlot = 50;

    [SerializeField] GameObject _SlotPrefab;
    [SerializeField] SlotMachineSlotDataSO[] _SlotDataSOs;

    // 데이터 풀 갯수
    int _MaxValue;

    [SerializeField] float _DecelMultiplier = 6f;
    [SerializeField] float _RollTic;

    [System.Serializable]
    private class LaneContext
    {
        public List<GameObject> SlotObj = new List<GameObject>();
        public RectTransform Content;
        public int MaxSlot;
        public float SlotHeight;
    }
    private LaneContext[] _LaneContext;

    [SerializeField] private int[] _Destiny;

    private bool _IsRolling = false;

    [Header("Button Setting")]
    [SerializeField] private Button _RerollBtn;
    [SerializeField] private Button _CloseBtn;

    [Header("Betting")]
    [SerializeField] private TMP_Dropdown _BetCurrencyDropdown;
    [SerializeField] private CurrencyDataPoolSO _CurrencyDataPoolSO;
    [SerializeField] private TMP_InputField _CurrencyInputField;
    [SerializeField] private int _3MatchMul;
    [SerializeField] private int _2MatchMul;


    private List<ItemDataSO> _CurrencyList;
    private int _BettingCurrencyID;
    private int _BettingCurrencyAmount;
    public bool IsOpen => gameObject.activeSelf;

    void Awake()
    {
        _RerollBtn.onClick.AddListener(OnClickRerollBtn);
        _CloseBtn.onClick.AddListener(OnClickCloseBtn);
    }

    void Start()
    {
        // 배팅 타입 드롭다운 설정
        _CurrencyList = _CurrencyDataPoolSO.GetAllCurrencies().ToList();

        _BetCurrencyDropdown.options
        = _CurrencyList.Select(
            c => new TMP_Dropdown.OptionData(c.ItemInfo._DisplayName)).ToList();

        _BettingCurrencyID = _CurrencyList[_BetCurrencyDropdown.value].ID;

        _BetCurrencyDropdown.onValueChanged.AddListener(
            idx => _BettingCurrencyID = _CurrencyList[idx].ID);

        _CurrencyInputField.characterValidation
        = TMP_InputField.CharacterValidation.Digit;

        // 설정
        _MaxValue = _SlotDataSOs.Length;
        _Destiny = new int[_LaneContents.Length];


        // 라인 상태 클래스 생성
        _LaneContext = new LaneContext[_LaneContents.Length];

        // 라인 상태 세팅
        for (int i = 0; i < _LaneContents.Length; i++)
        {
            _LaneContext[i] = new LaneContext
            {
                Content = _LaneContents[i],
                MaxSlot = GetLaneMaxSlot(i),
                SlotHeight = _SlotPrefab.GetComponent<RectTransform>().rect.height
            };

            // 라인에 슬롯 생성
            LaneAddSlot(i);
            // 초기 이미지 배열
            SetSlot(i);
        }
        

    }

    int GetLaneMaxSlot(int laneIndex)
    {
        return laneIndex switch
        {
            0 => _Lane0_MaxSlot,
            1 => _Lane1_MaxSlot,
            2 => _Lane2_MaxSlot,
            _ => 30
        };
    }

    void LaneAddSlot(int laneIndex)
    {
        for (int i = 0; i < _LaneContext[laneIndex].MaxSlot; i++)
        {
            var go = Instantiate(_SlotPrefab, _LaneContext[laneIndex].Content);
            _LaneContext[laneIndex].SlotObj.Add(go);
        }        
    }

    // 시작할때 1회 전체 세팅
    void SetSlot(int laneIndex)
    {       
        for (int i = 0; i < _LaneContext[laneIndex].MaxSlot; i++)
        {
            _LaneContext[laneIndex].SlotObj[i].
            GetComponent<SlotMachineSlotManager>().
            SetSlot(_SlotDataSOs[GetRandom(0, _MaxValue)]);
        }    
    }
    
    // 리롤마다 전체 세팅 후 최종값 설정
    void SetInit()
    {
        for (int i = 0; i < _LaneContents.Length; i++)
        {
            SetSlot(i);
        }       

        SetDestiny();
    }


    // 결정된 값
    void GetDestiny()
    {
        for (int i = 0; i < _Destiny.Length; i++)
        {
            _Destiny[i] = GetRandom(0, _MaxValue);
        }
    }

    void SetDestiny()
    {
        for (int i = 0; i < _LaneContents.Length; i++)
        {
            // 각 라인의 마지막 슬롯에 최종값 할당
            _LaneContext[i].SlotObj[_LaneContext[i].MaxSlot - 1].
            GetComponent<SlotMachineSlotManager>().
            SetSlot(_SlotDataSOs[_Destiny[i]]);
        }
    }

    void Reroll()
    {
        // 이미 돌아가고 있으면 안됨
        if (_IsRolling) return;

        // 베팅 실패 시 안됨(잔여금 부족)
        if (!SetBettingMoney())
        {
            return;
        }
        // 드롭다운 비활성
        _BetCurrencyDropdown.interactable = false;

        // 결과 값 결정
        GetDestiny();
        // 룰렛 초기화
        SetInit();

        for (int i = 0; i < _LaneContents.Length; i++)
        {
            StartCoroutine(StartRoll(i));
        }
    }

    IEnumerator StartRoll(int laneIndex)
    {
        _IsRolling = true;

        var context = _LaneContext[laneIndex];
        float height = context.SlotHeight;     // 슬롯 1칸 높이
        float quater = height * 0.25f;          // 1/4 칸(요구사항)
        int slotsToTravel = context.MaxSlot - 1;   // 추가 루프 없이 "마지막 슬롯"까지
        float targetY = slotsToTravel * height;

        // 출발점 리셋
        context.Content.anchoredPosition = Vector2.zero;

        // 감속: 남은 거리 비율에 따라 대기시간을 점점 늘린다 (step 크기는 항상 1/4칸 유지)
        // _RollTic는 최소 틱, _DecelMultiplier는 감속 강도(아래 직후 추가)
        float decelK(float progress01)
            => Mathf.Lerp(1f, _DecelMultiplier, Mathf.SmoothStep(0f, 1f, progress01));

        while (context.Content.anchoredPosition.y + 0.0001f < targetY)
        {
            float curY = context.Content.anchoredPosition.y;
            float nextY = curY + quater;
            if (nextY > targetY) nextY = targetY;    // 오버슈트 방지

            // 이동(1/4칸 고정)
            context.Content.anchoredPosition = new Vector2(context.Content.anchoredPosition.x, nextY);

            // 진행도 0~1
            float p = targetY <= 0f ? 1f : (nextY / targetY);
            float wait = _RollTic * decelK(p);       // 점점 느려지게

            yield return new WaitForSeconds(wait);
        }

        // 최종 스냅(혹시 모를 오차 제거)
        context.Content.anchoredPosition = new Vector2(context.Content.anchoredPosition.x, targetY);

        // ★ 오탈자 수정: laneIndex 사용
        if (laneIndex == _LaneContents.Length - 1)
        {
            GetReward();
            _IsRolling = false;
            _CurrencyInputField.interactable = true;
            _BetCurrencyDropdown.interactable = true;
        }
    
    }

    bool SetBettingMoney()
    {
        // 값을 입력 안했을 경우
        if (string.IsNullOrWhiteSpace(_CurrencyInputField.text))
        {
            // 경고 메세지 출력
            GameEvents.RaiseShowWarning("Need Betting!!");
            return false;
        }

        _BettingCurrencyAmount = int.Parse(_CurrencyInputField.text.ToString());

        // 0 원 입력
        if (_BettingCurrencyAmount == 0)
        {
            // 경고 메세지 출력
            GameEvents.RaiseShowWarning("Can`t 0 Betting!!");
            return false;
        }

        if (!GameEvents.RaiseRequestCurrencySpend(_BettingCurrencyID, _BettingCurrencyAmount))
        {
            return false;
        }

        // 넣은 돈 수정 불가
        _CurrencyInputField.interactable = false;

        return true;
    }


    // _Destiny 기반으로 같은 수 3개면 3Match , 2개면 2Match 배율
    void GetReward()
    {
        var groups = _Destiny.GroupBy(x => x);
        var maxCount = groups.Max(g => g.Count());

        switch (maxCount)
        {
            case 3:
                GameEvents.RaiseRequestCurrencyGain(
                    _BettingCurrencyID, _BettingCurrencyAmount * _3MatchMul);
                break;
            case 2:
                GameEvents.RaiseRequestCurrencyGain(
                    _BettingCurrencyID, _BettingCurrencyAmount * _2MatchMul);
                break;
            default:
                break;
        }
    }

    void OnClickRerollBtn()
    {
        Reroll();
    }

    void OnClickCloseBtn()
    {
        if (_IsRolling) return;

        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
    
    int GetRandom(int minIndex, int maxIndex) => Random.Range(minIndex, maxIndex);
}