using UnityEngine;

public class EmoteUIManager : MonoBehaviour, IEmoteUI
{
    [SerializeField] GameObject _EmoteSlot;
    EmoteSO[] _EmoteSOs;

    void Awake()
    {
        _EmoteSOs = EmoteManager._Inst.EmoteSOs;

        foreach(var item in _EmoteSOs)
        {
            var slot = Instantiate(_EmoteSlot, this.transform);
            slot.GetComponent<EmoteSlot>().Setup(item);
        }
    }    

    public bool IsOpen => gameObject.activeSelf;

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
}
