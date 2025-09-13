using UnityEngine;
using UnityEngine.UI;

public interface IButtonAction { void Execute(); }


public class ButtonActionRunner : MonoBehaviour
{
    Button _Button;

    void Awake()
    {
        _Button = GetComponent<Button>();
        _Button.onClick.AddListener(Run);
    }

    void OnDestroy()
    {
        if (_Button) _Button.onClick.RemoveListener(Run);
    }

    void Run()
    {
        var actions = GetComponents<IButtonAction>();
        for (int i = 0; i < actions.Length; i++) actions[i].Execute();
    }

}
