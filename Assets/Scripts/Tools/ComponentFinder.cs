using UnityEngine;

public static class ComponentFinder
{
    /// <summary>
    /// origin 기준으로 Self → Parents → Children 순서로 T를 찾는다.
    /// </summary>
    public static bool TryFindInSelfParentChildren<T>(Component origin, out T result) where T : class
    {
        // Self
        if (origin.TryGetComponent(out T self))
        {
            result = self;
            return true;
        }

        // Parents
        var parent = origin.GetComponentInParent<T>();
        if (parent != null && !ReferenceEquals(parent, origin))
        {
            result = parent;
            return true;
        }

        // Children
        var child = origin.GetComponentInChildren<T>();
        if (child != null && !ReferenceEquals(child, origin))
        {
            result = child;
            return true;
        }

        result = null;
        return false;
    }
}