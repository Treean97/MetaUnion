using UnityEngine;

[CreateAssetMenu(fileName = "UIAction", menuName = "ChoiceAction/OpenUIAction")]
public class DialogueOpenUIAction : ChoiceActionSO
{
    [SerializeField] private DialogueUIKey _TargetUI;

    public override void Execute()
    {
        var router = UIRouter._Inst;
        if (router == null)
        {
            Debug.LogWarning("[DialogueOpenUIAction] UIRouter 인스턴스가 없음");
            return;
        }

        router.Open(_TargetUI);   // 확장 메서드 사용
    }
}