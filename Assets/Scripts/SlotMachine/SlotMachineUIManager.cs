using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SlotMachineUIManager : MonoBehaviour
{
    [SerializeField] GameObject[] _Lanes;
    [SerializeField] GameObject[] _Masks;
    [SerializeField] private int _MaxIndex = 9;
    [SerializeField] private float _Speed = 200f;
    [SerializeField] private float _DefauleSpeed = 10f;
    [SerializeField] private int   _RollTime = 20;    // 반복 사이클 수
    private int _RequestCnt = 0;
    bool _IsRolling = false;

    [Header("Button")]
    [SerializeField] Button _RerollBtn;
    [SerializeField] Button _CloseBtn;

    // 속도 변경 이벤트
    public static event Action<float> OnSpeedChanged;
    public void RaiseSpeedChanged(float speed) => OnSpeedChanged?.Invoke(speed);    



    void Awake()
    {
        _RerollBtn.onClick.AddListener(RerollBtn);
        _CloseBtn.onClick.AddListener(CloseBtn);
    }

    void OnEnable()
    {
        // 초기 속도 설정
        RaiseSpeedChanged(_DefauleSpeed);
        SlotElement.OnRequestInfo += HandleRequestInfo;
    }

    void OnDisable()
    {
        SlotElement.OnRequestInfo -= HandleRequestInfo;
    }

    void Update()
    {
        // 돌아가는 중이고, 일정 바퀴수를 채웠다면
        if (_RequestCnt >= _RollTime && _IsRolling)
        {
            RaiseSpeedChanged(0);
            GetResult();
        }
    }

    void GetResult()
    {
        for (int i = 0; i < _Lanes.Length; i++)
        {
            Transform lane = _Lanes[i].transform;
            RectTransform maskRect = _Masks[i].GetComponent<RectTransform>();

            // 마스크 중앙의 월드 위치를 레인 로컬 좌표로 변환
            Vector3 worldCenter = maskRect.transform.position;
            Vector3 localCenter = lane.InverseTransformPoint(worldCenter);
            float centerY = localCenter.y;

            // 중앙에 가장 가까운 슬롯 찾기
            Transform closest = null;
            float minDist = float.MaxValue;
            foreach (Transform slot in lane)
            {
                float dist = Mathf.Abs(slot.localPosition.y - centerY);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = slot;
                }
            }

            if (closest != null)
            {
                var textComp = closest.GetComponentInChildren<TMP_Text>();
                Debug.Log($"Lane {i} result: {textComp.text}");
            }
        }
    }

    void HandleRequestInfo(GameObject obj)
    {
        obj.GetComponent<SlotElement>().SetInfo(GetRandom());
        // 할당 카운트 추가
        AddCount();
    }

    void RerollBtn()
    {
        if (_IsRolling) return;
        _IsRolling = true;

        // 스핀 시작 속도 설정
        RaiseSpeedChanged(_Speed);

        // 카운트 초기화
        _RequestCnt = 0;
    }

    void AddCount() => _RequestCnt++;    


    void CloseBtn()
    {
        if (_IsRolling) return;

        gameObject.SetActive(false);
    }

    int GetRandom() => UnityEngine.Random.Range(0, _MaxIndex);
}
