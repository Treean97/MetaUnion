using System.Collections;
using System.Data.Common;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineManager : MonoBehaviour
{
    public static TimelineManager _Inst { get; private set; }
    PlayableDirector _Director;

    bool _IsRunning;
    public bool IsRunning => _IsRunning;

    public enum ClickMode
    {
        DialogueAndTimeline,
        Timeline,
        Dialogue
    }
    ClickMode _Mode = ClickMode.DialogueAndTimeline;
    public ClickMode Mode => _Mode; 

    bool _IsBlockDialogue;
    public bool IsBlockDialogue => _IsBlockDialogue;

    void Awake()
    {
        if (_Inst != null && _Inst != this) { Destroy(gameObject); return; }
        _Inst = this;
    }

    void Start()
    {
        if (!_Director) _Director = GetComponent<PlayableDirector>();
        if (_Director) _Director.stopped += EndTimeline;
    }

    void OnDestroy()
    {
        if (_Director) _Director.stopped -= EndTimeline;
        if (_Inst == this) _Inst = null;
    }

    void OnEnable()  { DialogueUIManager.OnClick += ResumeTL; }
    void OnDisable() { DialogueUIManager.OnClick -= ResumeTL; }

    void BlockDialogue() => _IsBlockDialogue = true;
    void UnBlockDialogue() => _IsBlockDialogue = false;


    // 시그널 : 클릭 -> 타임라인, 대사 진행
    public void Signal_WaitClick_DialogueAndTimeline()
    {
        _Mode = ClickMode.DialogueAndTimeline;
        PauseTL();
        UnBlockDialogue(); // 클릭 입력 받게 허용
    }

    // 시그널 : 클릭 -> 타임라인 진행
    public void Signal_WaitClick_TimelineOnly()
    {
        _Mode = ClickMode.Timeline;
        PauseTL();
        UnBlockDialogue(); // 클릭 입력 받게 허용
    }

    // 시그널 : 클릭 -> 대사 진행
    public void Signal_WaitClick_DialogueOnly()
    {
        _Mode = ClickMode.Dialogue;
        PauseTL();
        UnBlockDialogue(); // 클릭 입력 받게 허용
    }

    // 외부 호출
    public void Play(PlayableAsset timeline)
    {
        if (!_Director || !timeline) { Debug.LogError("디렉터 or 타임라인 없음"); return; }
        StartCoroutine(CoPlayWithTransitions(timeline));
        _IsRunning = true;
    }

    IEnumerator CoPlayWithTransitions(PlayableAsset timeline)
    {
        var sem = ScreenEffectManager._Inst;

        // 대화 UI 잠시 꺼두기
        UIRouter._Inst.Close<IDialogueUI>();

        // 사전 블랙
        if (sem != null) yield return sem.FadeOut().WaitForCompletion();

        // 타임라인 준비
        _Director.playableAsset = timeline;
        _Director.RebuildGraph();
        _Director.time = 0;
        _Director.Evaluate();

        // 대사 차단 + 열면서 시작
        BlockDialogue();
        if (sem != null) yield return sem.FadeIn().WaitForCompletion();  // 타임라인 0프레임에서 화면 오픈
        _Director.Play();
        
        // 대화 UI 켜기
        UIRouter._Inst.Open<IDialogueUI>();
    }

    // 타임라인 종료
    void EndTimeline(PlayableDirector d)
    {
        StartCoroutine(CoEndSequence());
    }

    IEnumerator CoEndSequence()
    {       
        var sem = ScreenEffectManager._Inst;
        
        // 대화 UI 잠시 꺼두기
        UIRouter._Inst.Close<IDialogueUI>();

        // 페이드 아웃
        if (sem != null) yield return sem.FadeOut().WaitForCompletion();

        // 페이드 인
        if (sem != null) sem.FadeIn();

        // 대화 UI 켜기
        UIRouter._Inst.Open<IDialogueUI>();

        // 대사 허용 및 다음 대사
        DialogueManager._Inst.Next();
        UnBlockDialogue();
        _IsRunning = false;
    }

    // ===== 내부 유틸 =====
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
        BlockDialogue();
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
