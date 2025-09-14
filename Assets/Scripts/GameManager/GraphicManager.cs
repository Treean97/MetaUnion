using UnityEngine;

public class GraphicManager : MonoBehaviour, ISaveSection
{
    public static GraphicManager _Inst { get; private set; }

    public string Key => throw new System.NotImplementedException();

    public void ApplyJson(string s)
    {
        throw new System.NotImplementedException();
    }

    public string CaptureJson()
    {
        throw new System.NotImplementedException();
    }

    void Awake()
    {
        if (_Inst != null) { Destroy(gameObject); return; }
        _Inst = this;
        DontDestroyOnLoad(gameObject);
    }

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
