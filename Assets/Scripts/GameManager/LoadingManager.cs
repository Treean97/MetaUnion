using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;

public class LoadingManager : MonoBehaviour
{
    private static LoadingManager _Instance;

    [SerializeField] private Canvas _Canvas;
    [SerializeField] private Image _Background;
    [SerializeField] private Sprite[] _BackgroundPool;
    [SerializeField] private Slider _LoadingBar;
    [SerializeField] private TMP_Text _LoadingText;
    [SerializeField] private string[] _TextPool;

    bool _PausedPhotonQueue;

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
        SetBackground(); 
        _Canvas.gameObject.SetActive(true);
        if (_LoadingBar) _LoadingBar.value = 0f;

        if (_LoadingText)
        {
            if (_TextPool != null && _TextPool.Length > 0)
            {
                _LoadingText.text = _TextPool[Random.Range(0, _TextPool.Length)];
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

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.IsMessageQueueRunning = false;
            _PausedPhotonQueue = true;
        }

        StartCoroutine(CoLoadScene(sceneName));
    }

    IEnumerator CoLoadScene(string sceneName)
    {
        AudioManager._Inst?.SFXBlock();

        var op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            float target = Mathf.Clamp01(op.progress / 0.9f); // 0~1
            UpdateBar(target);
            yield return null;
        }

        // 마지막 구간 채우기
        yield return SmoothFillTo(1f, 1.5f);

        // 씬 활성화
        op.allowSceneActivation = true;
        yield return null; // 활성화 프레임 보장

        AudioManager._Inst?.SFXUnBlock();

        Hide();
    }

    void UpdateBar(float target)
    {
        if (!_LoadingBar) return;
        _LoadingBar.value = Mathf.Lerp(
            _LoadingBar.value, target,
            Time.unscaledDeltaTime * 5f
            );
    }

    IEnumerator SmoothFillTo(float target, float speed)
    {
        if (!_LoadingBar) yield break;
        while (_LoadingBar.value < target - 0.001f)
        {
            _LoadingBar.value = Mathf.Lerp(
                _LoadingBar.value,
                target,
                Time.unscaledDeltaTime * speed
                );
            yield return null;
        }

        if (_PausedPhotonQueue)
        {
            PhotonNetwork.IsMessageQueueRunning = true;
            _PausedPhotonQueue = false;
        }

        _LoadingBar.value = target;
    }

    void SetBackground()
    {
        if (_BackgroundPool == null || _BackgroundPool.Length == 0)
        {
            Debug.LogError("로딩 이미지풀 없음");
            return;
        }            
        var ran = Random.Range(0, _BackgroundPool.Length);
        _Background.sprite = _BackgroundPool[ran];
    }


}
