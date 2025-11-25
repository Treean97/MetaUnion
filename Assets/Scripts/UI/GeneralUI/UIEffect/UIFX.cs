using System.Collections;
using UnityEngine;

public static class UIFX
{
    class Host : MonoBehaviour
    {
        static Host _inst;
        public static Host Inst
        {
            get
            {
                if (_inst) return _inst;
                var go = new GameObject("[UIFX.Host]");
                DontDestroyOnLoad(go);
                _inst = go.AddComponent<Host>();
                return _inst;
            }
        }
    }

    // 켜기: 있으면 연출, 없으면 즉시 활성
    public static void Show(GameObject go)
    {
        if (!go) return;

        if (go.activeSelf)
            return;

        go.SetActive(true);

        if (go.TryGetComponent<UIPopEffect>(out var fx))
            fx.PlayShow();
    }

    // 끄기: 있으면 연출 후 비활성, 없으면 즉시 비활성
    public static void Hide(GameObject go)
    {
        if (!go) return;
        Host.Inst.StartCoroutine(CoHide(go));
    }

    static IEnumerator CoHide(GameObject go)
    {
        if (!go.activeSelf) yield break;
        if (go.TryGetComponent<UIPopEffect>(out var fx))
            yield return fx.PlayHide();
        else
            go.SetActive(false);
    }

}
