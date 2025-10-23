using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
    public static SceneLoadManager _Inst { get; private set; }

    ScreenEffectManager _ScreenEffect;

    void Awake()
    {
        if (_Inst != null && _Inst != this) { Destroy(gameObject); return; }
        _Inst = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        _ScreenEffect = ScreenEffectManager._Inst;
    }

    void OnDestroy()
    {
        if (_Inst == this) _Inst = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode mode)
    {
        // 다음 프레임: 새 씬 UI가 깔린 뒤 페이드 인
        StartCoroutine(CoOpenNextFrame());
    }

    IEnumerator CoOpenNextFrame()
    {
        yield return null;
        var sem = _ScreenEffect;
        if (sem != null) sem.FadeIn();
    }

    /// <summary>
    /// 닫힘(검정) → 로딩 시작 → 씬 전환(LoadingManager가 로드 수행)
    /// </summary>
    public void SceneLoad(string sceneName)
    {
        StartCoroutine(CoSceneLoad(sceneName));
    }

    IEnumerator CoSceneLoad(string sceneName)
    {
        var sem = _ScreenEffect;

        if (sem != null)
        {
            // 닫기 완료(검정)까지 대기
            yield return sem.FadeOut().WaitForCompletion();

            // 로딩 화면/프로그레스 UI를 보여주려면 오버레이 잠깐 숨김
            // (검정 유지가 목적이면 주석 처리)
            sem.SetOverlayVisible(false);
        }

        LoadingManager._Inst?.LoadScene(sceneName);
    }

    /// <summary>
    /// 컷씬/전환 등에서 "닫힌 상태 보장"이 필요할 때 사용.
    /// 닫히는 동안 대기하고, 검정 유지. 열기는 호출 측에서.
    /// </summary>
    public IEnumerator WaitDuringClose()
    {
        var sem = _ScreenEffect;
        if (sem == null) yield break;
        yield return sem.WaitDuringFadeOut();
    }
}
