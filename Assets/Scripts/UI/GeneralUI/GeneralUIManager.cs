using System.Collections;
using UnityEngine;

/// <summary>
/// 모든 UI 매니저의 중앙 허브 역할을 하는 싱글톤 컴포넌트
/// 씬에 하나만 배치하고, 각종 UI 매니저들을 참조하여 래핑된 접근 메서드를 제공합니다.
/// </summary>
[DisallowMultipleComponent]
public class GeneralUIManager : MonoBehaviour
{
    public static GeneralUIManager _Inst { get; private set; }

    [Header("General UI Canvas")]
    [SerializeField] private Canvas _GeneralUICanvas;

    [Header("Warning UI")]
    [SerializeField] private WarningUIManager _WarningUIPrefab;

    [Header("Focus UI")]
    [SerializeField] private FocusUIManager _FocusUIPrefab;


    private void Awake()
    {
        if (_Inst == null)
        {
            _Inst = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        // Warning UI
        GameEvents.OnShowWarning += HandleShowWarning;
        // Focus UI
        GameEvents.OnFocus += HandleFocus;
        GameEvents.OnDefocus += HandleDefocus;
    }

    private void OnDisable()
    {
        // Warning UI
        GameEvents.OnShowWarning -= HandleShowWarning;
        // Focus UI
        GameEvents.OnFocus -= HandleFocus;
        GameEvents.OnDefocus -= HandleDefocus;
    }


    /// <summary>
    /// 경고 메시지를 지정한 시간 동안 화면에 표시합니다.
    /// </summary>
    /// <param name="message">출력할 문자열</param>
    /// <param name="duration">표시할 시간(초)</param>
    public void HandleShowWarning(string message, float duration = 2f)
    {
        if (_WarningUIPrefab == null || _GeneralUICanvas == null)
        {
            Debug.LogWarning("Warning UI Prefab 또는 GeneralUICanvas가 할당되지 않았습니다.");
            return;
        }

        // Prefab으로부터 WarningUIManager 인스턴스 생성
        var warningInstance = Instantiate(_WarningUIPrefab, _GeneralUICanvas.transform);
        warningInstance.Show(message, duration);

        // duration 이후에 인스턴스 파괴
        StartCoroutine(DestroyAfter(warningInstance.gameObject, duration + 0.2f));
    }

    private IEnumerator DestroyAfter(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
            Destroy(obj);
    }

    private void HandleFocus(ObjectInfo objInfo)
    {
        if (_FocusUIPrefab == null || _GeneralUICanvas == null)
        {
            Debug.LogWarning("Focus UI Prefab 또는 GeneralUICanvas가 할당되지 않았습니다.");
            return;
        }

        // 정보 전달
        _FocusUIPrefab.Show(objInfo);
        _FocusUIPrefab.gameObject.SetActive(true);
    }

    private void HandleDefocus()
    {
        _FocusUIPrefab.gameObject.SetActive(false);
    }
}
