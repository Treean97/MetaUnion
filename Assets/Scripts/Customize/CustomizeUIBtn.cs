using UnityEngine;
using UnityEngine.UI;

public class CustomizeUIBtn : MonoBehaviour
{
    [SerializeField] Button _CustomizeUIBtn;
    [SerializeField] GameObject _CustomizeUI;

    void Awake()
    {
        _CustomizeUIBtn.onClick.AddListener(() => OnClickCustomizeButton());
    }

    void OnClickCustomizeButton()
    {
        _CustomizeUI.gameObject.SetActive(true);
    }
}
