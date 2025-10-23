using UnityEngine;
using UnityEngine.Playables;

public class TimelineController : MonoBehaviour
{
    public static TimelineController _Inst { get; private set; }

    [SerializeField] PlayableDirector _Director;

    void Awake()
    {
        if (_Inst != null && _Inst != this) { Destroy(this); return; }
        _Inst = this;

        if (!_Director) _Director = FindAnyObjectByType<PlayableDirector>();
    }

    void OnEnable()
    {
        DialogueUIManager._OnUserAdvance += ResumeTL;
        if (DialogueManager._Inst != null)
            DialogueManager._Inst.OnEnd += OnDialogueEnd;
    }

    void OnDisable()
    {
        DialogueUIManager._OnUserAdvance -= ResumeTL;
        if (DialogueManager._Inst != null)
            DialogueManager._Inst.OnEnd -= OnDialogueEnd;
    }

    void OnDialogueEnd()
    {
        // 대화가 완전히 종료될 때도 안전하게 재생
        ResumeTL();
    }

    // 타임라인 마커에서 호출
    public void Marker_Next()
    {
        PauseTL();
        DialogueManager._Inst?.Next();
    }

    public void Play(PlayableAsset timeline)
    {
        if (!_Director || !timeline) return;
        _Director.playableAsset = timeline;
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
        if (!HasValidGraph()) return;          // ★ 그래프 없으면 조용히 무시
        _Director.Play();
        SetSpeed(1);
        _Director.Evaluate();                  // (선택) 1프레임 안정화
    }

    void SetSpeed(float s)
    {
        if (!HasValidGraph()) return;          // ★ 여기서도 가드
        var root = _Director.playableGraph.GetRootPlayable(0);
        if (root.IsValid()) root.SetSpeed(s);
    }
}
