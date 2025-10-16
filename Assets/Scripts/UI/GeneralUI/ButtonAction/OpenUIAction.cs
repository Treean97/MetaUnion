using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ButtonActionRunner))]
public class OpenUIAction : MonoBehaviour, IButtonAction
{
    [SerializeField] GameObject[] _Targets;

    public void Execute()
    {
        if (_Targets == null || _Targets.Length == 0) return;

        foreach (var target in _Targets)
        {
            if (!target) continue;

            // 활성화
            if (!target.activeSelf) target.SetActive(true);

            // 연출 실행
            if (target.TryGetComponent<UIPopEffect>(out var effect))
            {
                effect.PlayShow();
            }
        }
    }
}
