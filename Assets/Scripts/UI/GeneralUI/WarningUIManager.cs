using System.Collections;
using TMPro;
using UnityEngine;

public class WarningUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _WarningText;

    private Coroutine _HideCoroutine;

    public void Show(string message, float duration)
    {
        _WarningText.text = message;
        gameObject.SetActive(true);

        if (_HideCoroutine != null)
            StopCoroutine(_HideCoroutine);

        _HideCoroutine = StartCoroutine(HideAfter(duration));
    }

    private IEnumerator HideAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        gameObject.SetActive(false);
        _HideCoroutine = null;
    }
}
