using UnityEngine;
using UnityEngine.UI;

public class CustomizePreivew : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RawImage _RawImage;
    [SerializeField] private Button _LeftButton;
    [SerializeField] private Button _RightButton;

    [Header("RT")]
    [SerializeField] private int _MinSize = 256;
    [SerializeField] private int _DepthBits = 16;
    [SerializeField] private int _Msaa = 1;
    [SerializeField] private bool _UseMip = true;

    [SerializeField] GameObject _CustomizePreviewPrefab;
    [SerializeField] Vector3 _PreviewPos;

    private GameObject _CustomizePreviewObj;
    private Camera _Camera;
    private CustomizeCamera _CustomizeCamera;
    private RenderTexture _RT;
    private PlayerPreviewRouter _Router;

    void Start()
    {
        _CustomizePreviewObj = Instantiate(_CustomizePreviewPrefab);
        _CustomizePreviewObj.transform.position = _PreviewPos;

        _Router = _CustomizePreviewObj.GetComponent<PlayerPreviewRouter>();
        _CustomizeCamera = _CustomizePreviewObj.GetComponentInChildren<CustomizeCamera>();
        _Camera = _CustomizePreviewObj.GetComponentInChildren<Camera>();
        

        EnsureRT();
        _Camera.targetTexture = _RT;
        _RawImage.texture = _RT;

        ChangeTarget(ItemType.Hair);
    }

    public void LeftButtonPointerDown()
    {
        _CustomizeCamera.OnLeftDown();
    }
    public void LeftButtonPointerUp()
    {
        _CustomizeCamera.OnLeftUp();
    }

    public void RightButtonPointerDown()
    {
        _CustomizeCamera.OnRightDown();
    }
    public void RightButtonPointerUp()
    {
        _CustomizeCamera.OnRightUp();
    }

    public void PlayPose(string id) => _Router?.Play(id);

    public void ChangeTarget(ItemType type)
    {
        _CustomizeCamera.SetSingleTarget(type);
    }

    private void EnsureRT()
    {
        if (!_RawImage) return;

        var rect = _RawImage.rectTransform.rect;
        int w = Mathf.Max(_MinSize, Mathf.RoundToInt(rect.width));
        int h = Mathf.Max(_MinSize, Mathf.RoundToInt(rect.height));

        if (_RT && _RT.width == w && _RT.height == h) return;

        if (_RT) { _RT.Release(); Destroy(_RT); }

        _RT = new RenderTexture(w, h, _DepthBits, RenderTextureFormat.DefaultHDR)
        {
            useMipMap = _UseMip,
            autoGenerateMips = _UseMip,
            antiAliasing = Mathf.Max(1, _Msaa),
            anisoLevel = 4,
            name = "CustomizePreview_RT"
        };
        _RT.Create();

        if (_Camera) _Camera.targetTexture = _RT;
        if (_RawImage) _RawImage.texture = _RT;
    }

}
