using UnityEngine;
using UnityEngine.UI;

public class VendingMachineUIManager : MonoBehaviour
{
    [SerializeField] ItemDataPoolSO _VendingMachineItemDataPoolSO;
    [SerializeField] Button _CloseBtn;

    void Awake()
    {
        _CloseBtn.onClick.AddListener(OnClickCloseBtn);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnClickCloseBtn()
    {
        gameObject.SetActive(false);
    }
}
