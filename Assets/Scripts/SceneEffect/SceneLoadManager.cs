using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
    public static SceneLoadManager _Inst { get; private set; }

    [Header("Refs")]
    [SerializeField] Canvas _Canvas; // 전역 캔버스 (프리팹 루트)
    [SerializeField] CircleRevealOverlay _Overlay; // 위 Canvas 자식 Image에 부착

    void Awake()
    {
        if (_Inst != null && _Inst != this) { Destroy(gameObject); return; }
        _Inst = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (_Inst == this) _Inst = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode mode)
    {
        // 다음 씬 UI가 깔린 뒤 열기
        StartCoroutine(CoOpenNextFrame());
    }

    IEnumerator CoOpenNextFrame()
    {
        yield return null;
        if (_Overlay) _Overlay.Open();
    }

    /// <summary>
    /// 검게 닫힘 → 로딩 시작 → 씬 전환 (LoadingManager가 로드 수행)
    /// </summary>
    public void SceneLoad(string sceneName)
    {
        StartCoroutine(CoSceneLoad(sceneName));
    }

    IEnumerator CoSceneLoad(string sceneName)
    {
        if (_Overlay)
            yield return _Overlay.Close().WaitForCompletion();

        // 로딩 화면을 보여주려면 오버레이 잠깐 끔
        if (_Overlay) _Overlay.SetVisible(false);

        LoadingManager._Inst?.LoadScene(sceneName);
    }
}
