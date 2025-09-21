using UnityEngine;
using UnityEngine.UI;

public interface IButtonAction { void Execute(); }


public class ButtonActionRunner : MonoBehaviour
{
    Button _Button;

    void Awake()
    {
        _Button = GetComponent<Button>();
    }

    public void Run()
    {
        var actions = GetComponents<IButtonAction>();
        for (int i = 0; i < actions.Length; i++) actions[i].Execute();
    }

}
