using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineManager : MonoBehaviour
{
    public static TimelineManager _Inst { get; private set; }

    PlayableDirector _Director;

    void Start()
    {
        if (!_Director) _Director = GetComponent<PlayableDirector>();
    }


    void OnEnable()
    {
        DialogueUIManager._OnUserAdvance += ResumeTL;
    }

    void OnDisable()
    {
        DialogueUIManager._OnUserAdvance -= ResumeTL;
    }

    // 타임라인 마커에서 호출
    public void Marker_Next()
    {
        PauseTL();
        DialogueManager._Inst?.Next();
    }

    public void Play(PlayableAsset timeline)
    {
        Debug.Log("타임라인 실행");
        if (!_Director || !timeline)
        {
            Debug.LogError("디렉터 or 타임라인 없음");
            return;
        }
        
        _Director.Stop();
        _Director.playableAsset = timeline;
                    
        _Director.time = 0;
        _Director.Play();        
    }

    bool HasValidGraph()
    {
        if (!_Director) return false;
        var g = _Director.playableGraph;
        return g.IsValid() && g.GetRootPlayableCount() > 0;
    }
    
    void PauseTL()
    {
        if (!HasValidGraph()) return;
        _Director.Pause();
        SetSpeed(0);
    }
    void ResumeTL()
    {
        if (!HasValidGraph()) return;
        _Director.Play();
        SetSpeed(1);
    }

    void SetSpeed(float s)
    {
        if (!HasValidGraph()) return;
        var root = _Director.playableGraph.GetRootPlayable(0);
        if (root.IsValid()) root.SetSpeed(s);
    }
}
