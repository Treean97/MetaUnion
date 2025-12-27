using UnityEngine;

public static class FindHelper
{
    /// <summary>
    /// origin 기준으로 부모 체인을 타면서, MonoBehaviour들 중 T(인터페이스 포함)를 구현한 첫 대상을 찾는다.
    /// </summary>
    public static T FindInterfaceInParent<T>(Transform origin, bool includeSelf = true) where T : class
    {
        if (origin == null) return null;

        Transform t = includeSelf ? origin : origin.parent;

        while (t != null)
        {
            var list = t.GetComponents<MonoBehaviour>();
            for (int i = 0; i < list.Length; i++)
            {
                var mb = list[i];
                if (mb == null) continue; // Missing Script 방어
                if (mb is T found) return found;
            }

            t = t.parent;
        }

        return null;
    }
}
