using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmoteSlot : MonoBehaviour, IItemDataProvider
{
    [SerializeField] Image _Icon;
    [SerializeField] TMP_Text _DisplayName;
    EmoteSO _EmoteSO;
    Button _Button;

    void Awake()
    {
        _Button = GetComponent<Button>();
        _Button.onClick.AddListener(OnClick);
    }

    public InfoDataSO GetItemData()
    {
        return _EmoteSO.InfoDataSO;
    }

    public void Setup(EmoteSO emoteSO)
    {
        _EmoteSO = emoteSO;
        _Icon.sprite = emoteSO.Icon;
        _DisplayName.text = emoteSO.InfoDataSO.DisplayName;
    }    

    void OnClick()
    {
        var mgr = EmoteManager._Inst;

        Vector3 pos = transform.position + transform.forward * 1.5f;
        Quaternion rot = Quaternion.LookRotation(-transform.forward, Vector3.up);

        var owner = PlayerSetup._LocalPlayer.GetComponent<PlayerEmote>();
        
        mgr.StartEmote(_EmoteSO, pos, rot, owner);
    }

}
