using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class SlotMachineUIManager : MonoBehaviour
{
    [SerializeField] GameObject[] _Lanes;

    [SerializeField] int _MaxIndex;

    [SerializeField] int _RollTime;

    int _ItemCnt = 0;

    int GetRandom(int minIndex, int maxIndex) => UnityEngine.Random.Range(minIndex, maxIndex);

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

    void Awake()
    {
        _RerollBtn.onClick.AddListener(OnClickRerollBtn);
        _CloseBtn.onClick.AddListener(OnClickCloseBtn);
    }

    void OnClickRerollBtn()
    {
        Reroll();
    }

    void OnClickCloseBtn()
    {
        gameObject.SetActive(false);
    }


    // 결정된 값
    void GetDestiny()
    {
        for (int i = 0; i < _Destiny.Length; i++)
        {
            _Destiny[i] = GetRandom(0, _MaxIndex);
        }
    }

    void Start()
    {
        _Destiny = new int[_Lanes.Length];
        _ItemCnt = _DisplayItemSlots[0].SlotObj.Count;

    }

    void SetRandomInit()
    {
        for (int i = 0; i < _Lanes.Length; i++)
        {
            // 마지막 전 까지 랜덤
            for (int j = 0; j < _ItemCnt - 1; j++)
            {
                _DisplayItemSlots[i].SlotObj[j].GetComponentInChildren<TMP_Text>().text
                = GetRandom(0, _MaxIndex).ToString();
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
                = GetRandom(0, _MaxIndex).ToString();
            }            
        }
    }

    void Reroll()
    {
        if (_IsRolling) return;

        GetDestiny();
        SetRandomInit();       

        for (int i = 0; i < _Lanes.Length; i++)
        {
            StartCoroutine(StartRoll(i));
        }
    }

    IEnumerator StartRoll(int slotIndex)
    {
        _IsRolling = true;

        // 인덱스 별 바퀴 수
        for (int i = 0; i < _RollTime * (slotIndex + 1); i++)
        {
            // 2번 움직임에 1칸 이동이고, 처음과 마지막은 같으니 (_ItemCnt - 1) * 2
            for (int j = 0; j < (_ItemCnt - 1) * 2; j++)
            {
                _Lanes[slotIndex].transform.localPosition -= new Vector3(0, 50f, 0);
                if (_Lanes[slotIndex].transform.localPosition.y < 0)
                {
                    _Lanes[slotIndex].transform.localPosition
                    += new Vector3(0, (_ItemCnt - 1) * 100f, 0);
                }
                yield return new WaitForSeconds(0.05f);

            }

            SetRandom();
        }

        _IsRolling = false;
    }
    

}