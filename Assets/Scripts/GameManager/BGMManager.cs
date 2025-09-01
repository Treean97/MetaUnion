using UnityEngine;

public class BGMManager : MonoBehaviour
{
    [SerializeField] private AudioClip _Clip;

    void Start()
    {
        if (SoundManager._Inst && _Clip)
            SoundManager._Inst.PlayBGM(_Clip);
    }
}
