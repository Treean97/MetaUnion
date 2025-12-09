using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class QuitButton : MonoBehaviour
{
    Button _Button;


    void Awake()
    {
        _Button = GetComponent<Button>();
        _Button.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
#if UNITY_EDITOR
        // 에디터에서는 플레이 모드 종료
        EditorApplication.isPlaying = false;
#else
        // 빌드된 게임에서는 앱 종료
        Application.Quit();
#endif
    }
}
