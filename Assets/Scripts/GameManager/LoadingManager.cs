using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    private static LoadingManager _Instance;

    [SerializeField] private Canvas _Canvas;
    [SerializeField] private Slider _LoadingBar;
    [SerializeField] private TMP_Text _LoadingText;
    [SerializeField] private string[] _TextSample;

    void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _Canvas.gameObject.SetActive(false);
    }

    public static LoadingManager _Inst => _Instance;


    public void Show()
    {
        _Canvas.gameObject.SetActive(true);
        if (_LoadingBar) _LoadingBar.value = 0f;

        if (_LoadingText)
        {
            if (_TextSample != null && _TextSample.Length > 0)
            {
                _LoadingText.text = _TextSample[UnityEngine.Random.Range(0, _TextSample.Length)];
            }
            else
            {
                _LoadingText.text = "Text Error";
            }

        }
    }

    public void Hide()
    {
        _Canvas.gameObject.SetActive(false);
    }


    public void LoadScene(string sceneName)
    {
        Show();
        StopAllCoroutines();
        StartCoroutine(CoLoadScene(sceneName));
    }

    IEnumerator CoLoadScene(string sceneName)
    {
        var op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            float target = Mathf.Clamp01(op.progress / 0.9f); // 0~1
            UpdateBar(target);
            yield return null;
        }

        // 마지막 구간 채우기
        yield return SmoothFillTo(1f, 5f);

        // 씬 활성화
        op.allowSceneActivation = true;
        yield return null; // 활성화 프레임 보장

        Hide();
    }

     void UpdateBar(float target)
    {
        if (!_LoadingBar) return;
        _LoadingBar.value = Mathf.Lerp(_LoadingBar.value, target, Time.deltaTime * 5f);
    }

    IEnumerator SmoothFillTo(float target, float speed)
    {
        if (!_LoadingBar) yield break;
        while (_LoadingBar.value < target - 0.001f)
        {
            _LoadingBar.value = Mathf.Lerp(_LoadingBar.value, target, Time.deltaTime * speed);
            yield return null;
        }
        _LoadingBar.value = target;
    }


}
