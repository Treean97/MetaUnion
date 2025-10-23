using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueChoiceItem : MonoBehaviour
{
    [SerializeField] TMP_Text _Label;
    [SerializeField] Button _Button;

    public void Bind(string text, System.Action onClick)
    {
        if (_Label) _Label.text = text ?? "";
        _Button.onClick.RemoveAllListeners();
        _Button.onClick.AddListener(() => onClick?.Invoke());
        gameObject.SetActive(true);
    }
}
