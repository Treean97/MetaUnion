using UnityEngine;

public class BGMManager : MonoBehaviour
{
    [SerializeField] private AudioClip _Clip;

    void Start()
    {
        if (AudioManager._Inst && _Clip)
            AudioManager._Inst.PlayBGM(_Clip);
    }
}
