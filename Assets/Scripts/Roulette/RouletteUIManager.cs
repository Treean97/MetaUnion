using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RouletteUIManager : MonoBehaviour, IRouletteUI
{    
    [SerializeField] ItemDataPoolSO _RouletteItemPool;
    [SerializeField] Transform _RoulettePointer;
    [SerializeField] GameObject _Spinner;
    [SerializeField] int _RouletteSlotCount;    
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


    public bool IsOpen => gameObject.activeSelf;

    void Awake()
    {
        _SpinBtn.onClick.AddListener(OnClickSpinBtn);
        _CloseBtn.onClick.AddListener(OnClickCloseBtn);
    }

    void Start()
    {
        _Slots.Clear();
        float angleTerm = 360f / _RouletteSlotCount;

        // 룰렛 슬롯 배치
        for (int i = 0; i < _RouletteSlotCount; i++)
        {
            GameObject slot = Instantiate(_RouletteSlotPrefab, _Spinner.transform);

            float angle = i * angleTerm - 90f;
            float rad = angle * Mathf.Deg2Rad;

            Vector2 pos = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * _Radius;

            slot.transform.localPosition = pos;

            _Slots.Add(slot);        
        }
    }

    void Update()
    {
        if (!_IsSpin)
        {
            _Spinner.transform.Rotate(Vector3.forward, _NormalSpeed * Time.deltaTime);
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
        GameObject closestSlot = null;
        float closestDis = float.MaxValue;

        foreach (var slot in _Slots)
        {
            float dis
            = Vector3.Distance(slot.transform.position, _RoulettePointer.transform.position);
            if (closestDis > dis)
            {
                closestSlot = slot;
                closestDis = dis;
            }
        }

        return closestSlot;        
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

        float time = 0;
        while (time <= _Duration)
        {
            float speed = Mathf.Lerp(_MaxSpeed, 0f, time / _Duration);
            _Spinner.transform.Rotate(Vector3.forward, speed * Time.deltaTime);
            time += Time.deltaTime;
            yield return null;        
        }

        // 아이템 선택
        GameObject reward = SelectSlot();
        GameEvents.RaiseRequestItemGain(
            reward.GetComponent<RouletteSlot>().ItemDataSO.ID,
            reward.GetComponent<RouletteSlot>().Amount);

        // 아이템 확인 시간 
        yield return new WaitForSeconds(_WaitToNextSpin);
        _IsSpin = false;
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
