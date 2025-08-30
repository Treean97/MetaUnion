using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SlotMachineUIManager : MonoBehaviour, ISlotMachineUI
{
    [SerializeField] GameObject[] _Lanes;

    [SerializeField] int _MaxValue;

    [SerializeField] int _RollTime;
    [SerializeField] float _RollTic;

    int _ItemCnt = 0;

    int GetRandom(int minIndex, int maxIndex) => Random.Range(minIndex, maxIndex);

    [System.Serializable]
    public class DisplayItemSlot
    {
        public List<GameObject> SlotObj = new List<GameObject>();
    }
    public DisplayItemSlot[] _DisplayItemSlots;
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

    void OnEnable()
    {
        UIRouter._Inst?.RegisterAs<ISlotMachineUI>(this);
    }

    void OnDisable()
    {
        UIRouter._Inst?.UnregisterAs<ISlotMachineUI>(this);
    }

    void Start()
    {
        _CurrencyList = _CurrencyDataPoolSO.GetAllCurrencies().ToList();

        // 배팅 타입 드롭다운
        _BetCurrencyDropdown.options
        = _CurrencyList.Select(
            c => new TMP_Dropdown.OptionData(c.ItemInfo._DisplayName)).ToList();

        _BettingCurrencyID = _CurrencyList[_BetCurrencyDropdown.value].ID;

        _BetCurrencyDropdown.onValueChanged.AddListener(
            idx => _BettingCurrencyID = _CurrencyList[idx].ID);

        _Destiny = new int[_Lanes.Length];
        _ItemCnt = _DisplayItemSlots[0].SlotObj.Count;
        _CurrencyInputField.characterValidation
        = TMP_InputField.CharacterValidation.Digit;
    }

    void Update()
    {
        // 평상 시 돌아가는 것 처럼 구현
        if (!_IsRolling)
        {
            return;
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


    // 결정된 값
    void GetDestiny()
    {
        for (int i = 0; i < _Destiny.Length; i++)
        {
            _Destiny[i] = GetRandom(0, _MaxValue + 1);
        }
    }


    void SetRandomInit()
    {
        for (int i = 0; i < _Lanes.Length; i++)
        {
            // 마지막 전 까지 랜덤
            for (int j = 0; j < _ItemCnt - 1; j++)
            {
                _DisplayItemSlots[i].SlotObj[j].GetComponentInChildren<TMP_Text>().text
                = GetRandom(0, _MaxValue + 1).ToString();
            }
            // 마지막은 첫번째와 같음
            _DisplayItemSlots[i].SlotObj[_ItemCnt - 1].GetComponentInChildren<TMP_Text>().text
            = _DisplayItemSlots[i].SlotObj[0].GetComponentInChildren<TMP_Text>().text;
        }

        SetDestiny();
    }

    void SetDestiny()
    {
        for (int i = 0; i < _Destiny.Length; i++)
        {
            _DisplayItemSlots[i].SlotObj[0].GetComponentInChildren<TMP_Text>().text
            = _Destiny[i].ToString();
        }
    }

    void SetRandom()
    {
        for (int i = 0; i < _Lanes.Length; i++)
        {
            // 1 ~ 마지막-1 까지 랜덤
            for (int j = 1; j < _ItemCnt - 1; j++)
            {
                _DisplayItemSlots[i].SlotObj[j].GetComponentInChildren<TMP_Text>().text
                = GetRandom(0, _MaxValue + 1).ToString();
            }
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
        SetRandomInit();

        for (int i = 0; i < _Lanes.Length; i++)
        {
            StartCoroutine(StartRoll(i));
        }
    }

    IEnumerator StartRoll(int slotIndex)
    {
        // 돌아가는 중 다시 돌리기 방지
        _IsRolling = true;


        // 인덱스 별 바퀴 수
        for (int i = 0; i < _RollTime * (slotIndex + 1); i++)
        {
            // 2번 움직임에 1칸 이동이고, 처음과 마지막은 같으니 (_ItemCnt - 1) * 2
            for (int j = 0; j < (_ItemCnt - 1) * 4; j++)
            {
                _Lanes[slotIndex].transform.localPosition -= new Vector3(0, 25f, 0);
                if (_Lanes[slotIndex].transform.localPosition.y < 0)
                {
                    _Lanes[slotIndex].transform.localPosition
                    += new Vector3(0, (_ItemCnt - 1) * 100f, 0);
                }
                yield return new WaitForSeconds(_RollTic);

            }

            SetRandom();
        }


        // 마지막 룰렛이 종료될 때만 실행
        if (slotIndex == _Lanes.Length - 1)
        {
            GetReward();
            _IsRolling = false;
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

        _CurrencyInputField.interactable = true;
        _BetCurrencyDropdown.interactable = true;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}