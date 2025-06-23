using UnityEngine;
using UnityEngine.UI;

public class CustomizeUIBtn : MonoBehaviour
{
    [SerializeField] Button _CustomizeUIBtn;

    void OnEnable()
    {
        _CustomizeUIBtn.onClick.AddListener(() => GameEvents.RaiseRequestOpenCustomizeUI());
    }

}
