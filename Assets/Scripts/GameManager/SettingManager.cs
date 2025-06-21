using UnityEngine;

public class SettingManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Application.targetFrameRate = 144;

        Screen.SetResolution(1920, 1080, false); // false면 창모드, true면 전체화면
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
