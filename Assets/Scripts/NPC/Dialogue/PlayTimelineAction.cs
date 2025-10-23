using UnityEngine;
using UnityEngine.Playables;
[CreateAssetMenu (fileName = "PlayTimelineAction", menuName = "ChoiceAction/PlayTimelineAction")]
public class PlayTimelineAction : ChoiceActionSO
{
    // 실행 할 타임라인 
    [SerializeField] PlayableAsset _Timeline;

    public override void Execute()
    {
        // 타임라인 호출
        if (!_Timeline) return;

        Debug.Log("대사 선택지 액션 실행");

        TimelineController._Inst?.Play(_Timeline);        
    }
}
