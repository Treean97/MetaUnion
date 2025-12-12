using UnityEditor;
using UnityEngine;

public class TextureSizeChecker
{
    [MenuItem("Tools/Check Large Textures (>= 8192)")]
    public static void CheckLargeTextures()
    {
        const int limit = 8192;
        string[] guids = AssetDatabase.FindAssets("t:Texture2D");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null) continue;

            if (tex.width >= limit || tex.height >= limit)
            {
                Debug.LogWarning(
                    $"[LargeTexture] {path} : {tex.width}x{tex.height}", tex);
            }
        }

        Debug.Log("Large texture check finished.");
    }
}
