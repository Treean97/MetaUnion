using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "ScriptableObject/Scene List")]
public class SceneList : ScriptableObject
{
    [System.Serializable]
    public class SceneEntry
    {
        
#if UNITY_EDITOR
        public SceneAsset SceneAsset;
#endif
        
        public string SceneName;
    }

    public List<SceneEntry> _SceneList = new List<SceneEntry>();

#if UNITY_EDITOR
    // 자동화 : Unity 에디터 전용 콜백 메서드로, 인스펙터에서 스크립트의 직렬화된 값이 바뀔 때마다 자동으로 호출됩니다.
    private void OnValidate()
    {
        // 인스펙터에서 SceneList가 수정될 때마다 호출됩니다.
        foreach (var entry in _SceneList)
        {
            if (entry.SceneAsset != null)
                entry.SceneName = entry.SceneAsset.name;
        }

        // 변경 사항을 에디터에 반영
        EditorUtility.SetDirty(this);
    }
#endif
}
