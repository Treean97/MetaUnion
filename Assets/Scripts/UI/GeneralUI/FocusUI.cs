using TMPro;
using UnityEngine;

public class FocusUI : MonoBehaviour
{
    [SerializeField] private RectTransform _FocusUI;
    [SerializeField] private TMP_Text _Name;
    [SerializeField] private TMP_Text _Desc;

    void Awake()
    {
        FocusableUI.OnPointerEnterFocusUI += HandleOnPointerEnterFocusUI;
        FocusableUI.OnPointerExitFocusUI += HandleOnPointerExitFocusUI;
    }

    void OnDestroy()
    {
        FocusableUI.OnPointerEnterFocusUI -= HandleOnPointerEnterFocusUI;
        FocusableUI.OnPointerExitFocusUI -= HandleOnPointerExitFocusUI;
    }

    void HandleOnPointerEnterFocusUI(InfoDataSO itemInfo)
    {
        if (itemInfo == null) return;
        
        _Name.text = itemInfo.DisplayName;
        _Desc.text = itemInfo.Description;
        _FocusUI.gameObject.SetActive(true);
    }

    void HandleOnPointerExitFocusUI()
    {
        _FocusUI.gameObject.SetActive(false);
        _Name.text = "";
        _Desc.text = "";
    }


    // Update is called once per frame
    void Update()
    {
        if (!_FocusUI.gameObject.activeSelf)
        {
            return;
        }

        Vector2 mousePos = Input.mousePosition;

        // 패널 크기 (px)
        float w = _FocusUI.rect.width;
        float h = _FocusUI.rect.height;

        // 화면 경계 내로 보정
        float x = Mathf.Clamp(mousePos.x, 0, Screen.width - w);
        float y = Mathf.Clamp(mousePos.y, 0, Screen.height - h);

        // 위치 적용
        _FocusUI.transform.position = new Vector3(x, y, 0);    
    }
}
