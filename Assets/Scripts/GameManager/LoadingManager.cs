using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager _Inst { get; private set; }

    [SerializeField] private Slider _LoadingBar;
    [SerializeField] private TMP_Text _LoadingText;

    void Awake()
    {
        if (_Inst == null)
        {
            _Inst = this;            
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }


    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadingSceneAsync(sceneName));
    }

    IEnumerator LoadingSceneAsync(string sceneName)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;

        float progress = 0f;

        while (!asyncOperation.isDone)
        {
            yield return null;

            if (asyncOperation.progress < 0.9f)
            {
                _LoadingBar.value = asyncOperation.progress;
            }
            else
            {
                progress += Time.deltaTime;
                _LoadingBar.value = Mathf.Lerp(0.9f, 1f, progress);

                if (_LoadingBar.value >= 1f)
                {
                    asyncOperation.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }
}
