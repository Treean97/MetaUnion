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
        var owner = PlayerSetup._LocalPlayer.GetComponent<PlayerEmote>();
        var ownerTransform = owner.transform;
        Vector3 pos = ownerTransform.position + ownerTransform.forward * 1.5f;
        Quaternion rot = Quaternion.LookRotation(-ownerTransform.forward, Vector3.up);

        mgr.StartEmote(_EmoteSO, pos, rot, owner);
        
        UIRouter._Inst.Close<IEmoteUI>();
    }

}
