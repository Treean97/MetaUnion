using System;
using TMPro;
using UnityEngine;

public class SlotElement : MonoBehaviour
{
    [SerializeField] private TMP_Text _Text;

    [SerializeField] private float _ThresholdY = -270f;
    [SerializeField] private float _ReturnHeight = -50f;
    private float _Speed;

    // 추후 수정
    private int _ID;

    public static event Action<GameObject> OnRequestInfo;
    public void RaiseRequestInfo(GameObject obj) => OnRequestInfo?.Invoke(obj);


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        SlotMachineUIManager.OnSpeedChanged += OnSpeedChanged;
    }

    void OnDisable()
    {
        SlotMachineUIManager.OnSpeedChanged -= OnSpeedChanged;
    }

    void OnSpeedChanged(float Speed)
    {
        _Speed = Speed;
    }

    // Update is called once per frame
    void Update()
    {
        // 아래로 내리기
        transform.localPosition += Vector3.down * _Speed;

        // 최대로 내려가면 다시 위로
        if (transform.localPosition.y <= _ThresholdY)
        {
            transform.localPosition += Vector3.up * (_ReturnHeight - _ThresholdY);
            // 새로운 정보 할당
            RaiseRequestInfo(gameObject);
        }
    }

    public void SetInfo(int id)
    {
        _ID = id;
        _Text.text = _ID.ToString();
    }
}
