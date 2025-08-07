using UnityEngine;
using UnityEngine.UI;

public class InventoryDragSlot : MonoBehaviour
{
    [SerializeField] private Image _Icon;

    void Update()
    {
        _Icon.transform.position = Input.mousePosition;
    }

}
