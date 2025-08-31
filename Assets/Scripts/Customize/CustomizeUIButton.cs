using UnityEngine;
using UnityEngine.UI;

public class CustomizeUIButton : MonoBehaviour
{
    private Button _Button;

    void Awake()
    {
        _Button = GetComponent<Button>();
        _Button.onClick.AddListener(() => UIRouter._Inst.Open<ICustomizeUI>());
    }
}
