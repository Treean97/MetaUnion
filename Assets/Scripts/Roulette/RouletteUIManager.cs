using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RouletteUIManager : MonoBehaviour, IRouletteUI
{    
    [SerializeField] ItemDataPoolSO _RouletteItemPool;
    [SerializeField] Transform _RoulettePointer;
    [SerializeField] GameObject _Spinner;
    [SerializeField] int _RouletteSlotCount = 8;    
    [SerializeField] GameObject _RouletteSlotPrefab;
    [SerializeField] List<GameObject> _Slots;
    [SerializeField] float _Radius;
    [SerializeField] float _Duration;
    [SerializeField] float _MaxSpeed;
    [SerializeField] float _NormalSpeed;
    float _WaitToNextSpin = 3f;
    bool _IsSpin = false;

    [SerializeField] Button _SpinBtn;
    [SerializeField] Button _CloseBtn;

    [SerializeField] private string _RouletteSpinKey = "RouletteSpin";
    [SerializeField] private string _RewardSuccessKey = "RouletteRewardSuccess";


    public bool IsOpen => gameObject.activeSelf;

    void Awake()
    {
        _SpinBtn.onClick.AddListener(OnClickSpinBtn);
        _CloseBtn.onClick.AddListener(OnClickCloseBtn);
    }

    void Start()
    {
        if (_Slots == null)
        {
            _Slots = new List<GameObject>();
        }

        _Slots.Clear();

        float angleTerm = 360f / _RouletteSlotCount;
        float startAngleDeg = -90f + angleTerm * 0.5f;

        // 룰렛 슬롯 배치        
        // for (int i = 0; i < _RouletteSlotCount; i++)
        // {
        //     GameObject slot = Instantiate(_RouletteSlotPrefab, _Spinner.transform);

        //     float angle = i * angleTerm - 90f;
        //     float rad = angle * Mathf.Deg2Rad;

        //     Vector2 pos = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * _Radius;

        //     slot.transform.localPosition = pos;

        //     _Slots.Add(slot);        
        // }


        for (int i = 0; i < _RouletteSlotCount; i++)
        {
            GameObject slot = Instantiate(_RouletteSlotPrefab, _Spinner.transform);

            float angle = startAngleDeg + i * angleTerm;
            float rad = angle * Mathf.Deg2Rad;

            Vector2 pos = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * _Radius;

            var rt = slot.GetComponent<RectTransform>();   // UI 전제
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0f);              // 하단 중앙
            rt.anchoredPosition = pos;

            // 하단이 중앙을 가리키도록(슬롯의 위쪽이 바깥쪽 향하게)
            rt.localEulerAngles = new Vector3(0f, 0f, angle - 90f);

            _Slots.Add(slot);
        }

        DefaultSet();        
    }

    void Update()
    {
        if (!_IsSpin)
        {
            _Spinner.transform.Rotate(Vector3.back, _NormalSpeed * Time.deltaTime);
        }
    }

    void ItemSet()
    {
        for (int i = 0; i < _Slots.Count; i++)
        {
            // 랜덤 아이템 인덱스
            int randomIndex = Random.Range(0, _RouletteItemPool.GetItemCount());
            Debug.Log("PoolIndex:" + randomIndex);
            ItemDataSO itemDataSO = _RouletteItemPool.GetItemAt(randomIndex);
            // 랜덤 아이템 수량
            int randomAmount = Random.Range(1, 100);

            _Slots[i].GetComponent<RouletteSlot>().SetSlot(itemDataSO, randomAmount);
        }
    }

    GameObject SelectSlot()
    {
        GameObject closest = null;
        float min = float.MaxValue;
        Vector2 pointerPos = _RoulettePointer.position;

        foreach (var slot in _Slots)
        {
            Vector2 slotPos = slot.transform.position;
            float dis = (slotPos - pointerPos).sqrMagnitude; // 2D + sqrt 없음
            if (dis < min) { min = dis; closest = slot; }
        }

        return closest;           
    }

    void OnClickSpinBtn()
    {
        if (_IsSpin)
        {
            return;
        }
        ItemSet();
        StartCoroutine(RotateRoulette());
    }

    void OnClickCloseBtn()
    {
        if (_IsSpin)
        {
            return;
        }

        Hide();
    }

    IEnumerator RotateRoulette()
    {
        _IsSpin = true;

        // 스핀 사운드 시작
        var spinLoopPlayer = AudioManager._Inst?.Play2DLoopLocalPlayByKey(_RouletteSpinKey);

        float time = 0;
        while (time <= _Duration)
        {
            float speed = Mathf.Lerp(_MaxSpeed, 0f, time / _Duration);
            _Spinner.transform.Rotate(Vector3.back, speed * Time.deltaTime);
            time += Time.deltaTime;
            yield return null;
        }

        // 스핀 사운드 종료
        spinLoopPlayer.StopAndReturn();

        // 아이템 선택
        GameObject reward = SelectSlot();
        GameEvents.RaiseRequestItemGain(
            reward.GetComponent<RouletteSlot>().ItemDataSO.ID,
            reward.GetComponent<RouletteSlot>().Amount);

        // 보상 사운드 실행
        AudioManager._Inst.PlayLocalByKey(_RewardSuccessKey);

        // 아이템 확인 시간 
        yield return new WaitForSeconds(_WaitToNextSpin);
        _IsSpin = false;
        DefaultSet();
    }

    void DefaultSet()
    {
        foreach (var slot in _Slots)
        {
            slot.GetComponent<RouletteSlot>().DefaultSet();
        }
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
