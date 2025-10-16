using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ButtonActionRunner))]
public class CloseUIAction : MonoBehaviour, IButtonAction
{
    [SerializeField] GameObject[] _Targets;

    public void Execute()
    {
        if (_Targets == null || _Targets.Length == 0) return;
        foreach (var target in _Targets)
        {
            if (!target) continue;
            // 효과 후 비활성
            if (target.TryGetComponent<UIPopEffect>(out var anim))
            {
                StartCoroutine(CoClose(anim)); // Execute는 void라 내부에서 코루틴 시작
            }
            else
            {
                // 효과 컴포넌트가 없으면 즉시 끔(기존 동작 유지)
                target.SetActive(false);
            }
        }
    }

    IEnumerator CoClose(UIPopEffect anim)
    {
        yield return anim.PlayHide();
    }
}
