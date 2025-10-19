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
        var so = _EmoteSO;
        var owner = PlayerSetup._LocalPlayer.GetComponent<PlayerEmote>();

        if (so.PlayMode == EmotePlayMode.Solo)
        {
            // owner.RequestStartSolo(so); // 솔로 이모트
            return;
        }
        else // EmotePlayMode.Group
        {
            StartGroup(so, owner); // 단체 이모트
        }
        
        GetComponent<FocusableUI>().Defocus();
        UIRouter._Inst.Close<IEmoteUI>();
    }

    void StartGroup(EmoteSO so, PlayerEmote owner)
    {
        var mgr = EmoteManager._Inst;
        var t = owner.transform;
        Vector3 pos = t.position + t.forward * 1.5f;
        Quaternion rot = Quaternion.LookRotation(-t.forward, Vector3.up);
        mgr.StartEmote(so, pos, rot, owner);
    }


}
