using Com.MyCompany.MyGame;
using UnityEngine;
using UnityEngine.UI;

public class LeaveButton : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => GameManager._Inst.LeaveRoom());
        
    }
}
