using TMPro;
using UnityEngine;

public class VendingMachineFocusUIManager : MonoBehaviour
{
    [SerializeField] private RectTransform _VendingMachineFocusUI;
    [SerializeField] private TMP_Text _Name;
    [SerializeField] private TMP_Text _Description;

    public void Show(ItemInfoSO objInfo)
    {
        _Name.text = objInfo._DisplayName;
        _Description.text = objInfo._Description;
    }


    void Update()
    {
        if (!_VendingMachineFocusUI.gameObject.activeSelf)
        {
            return;
        }

        Vector2 mousePos = Input.mousePosition;

        // 패널 크기 (px)
        float w = _VendingMachineFocusUI.rect.width;
        float h = _VendingMachineFocusUI.rect.height;

        // 화면 경계 내로 보정
        float x = Mathf.Clamp(mousePos.x, 0, Screen.width - w);
        float y = Mathf.Clamp(mousePos.y, 0, Screen.height - h);

        // 위치 적용
        _VendingMachineFocusUI.position = new Vector3(x, y, 0);    
    }
}
