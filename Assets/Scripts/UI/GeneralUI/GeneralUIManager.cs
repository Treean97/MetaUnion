using System.Collections;
using Photon.Pun;
using UnityEngine;

[DisallowMultipleComponent]
public class GeneralUIManager : MonoBehaviourPun
{
    public static GeneralUIManager _Inst { get; private set; }

    [Header("General UI Canvas")]
    [SerializeField] private Canvas _GeneralUICanvas;

    [Header("Warning UI")]
    [SerializeField] private WarningUIManager _WarningUI;


    private void Awake()
    {
        if (_Inst == null)
        {
            _Inst = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
            return;
        }
    }


    private void OnEnable()
    {
        // Warning UI
        GameEvents.OnShowWarning += HandleShowWarning;
        GameEvents.OnHideWarning += HandleHideWarning;
    }

    private void OnDisable()
    {
        // Warning UI
        GameEvents.OnShowWarning -= HandleShowWarning;
        GameEvents.OnHideWarning -= HandleHideWarning;
    }


    /// <param name="message">출력할 문자열</param>
    /// <param name="duration">표시할 시간(초)</param>
    public void HandleShowWarning(string message, float duration = 2f)
    {
        _WarningUI.Show(message, duration);
    }

    void HandleHideWarning()
    {
        _WarningUI.Hide();
    }

}
