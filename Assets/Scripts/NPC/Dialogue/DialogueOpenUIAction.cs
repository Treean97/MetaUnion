using UnityEngine;

[CreateAssetMenu(fileName = "UIAction", menuName = "ChoiceAction/OpenUIAction")]
public class DialogueOpenUIAction : ChoiceActionSO
{
    [SerializeField] private string _PanelID;   // 열고 싶은 UI 패널 ID (문자열)

    public override void Execute()
    {
        if (string.IsNullOrEmpty(_PanelID)) return;

        if (DialogueUIPanel.TryGet(_PanelID, out var panel))
        {
            panel.Show();
        }
        else
        {
            Debug.LogWarning($"[DialogueOpenUIAction] '{_PanelID}' 패널을 찾을 수 없습니다.");
        }
    }
}
