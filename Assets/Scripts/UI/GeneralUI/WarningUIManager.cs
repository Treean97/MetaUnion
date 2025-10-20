using System.Collections;
using TMPro;
using UnityEngine;

public class WarningUIManager : MonoBehaviour
{
    [SerializeField] private GameObject _WarningUIPanel;
    [SerializeField] private TMP_Text _WarningText;

    private Coroutine _HideCoroutine;

    public void Show(string message, float duration)
    {      
        _WarningText.text = message;
        UIFX.Show(_WarningUIPanel);

        if (_HideCoroutine != null)
            StopCoroutine(_HideCoroutine);

        _HideCoroutine = StartCoroutine(HideAfter(duration));
    }

    private IEnumerator HideAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Hide();
        _HideCoroutine = null;
    }

    public void Hide()
    {
        UIFX.Hide(_WarningUIPanel);
    }
}
