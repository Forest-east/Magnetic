using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ResaveOldAssets
{
    [MenuItem("Tools/Resave Prefabs Animations Materials")]
    public static void Resave()
    {
        string[] paths = AssetDatabase.GetAllAssetPaths()
            .Where(path => path.StartsWith("Assets/") &&
                (path.EndsWith(".prefab") ||
                 path.EndsWith(".controller") ||
                 path.EndsWith(".anim") ||
                 path.EndsWith(".mat")||
                 path.EndsWith(".unity")))
            
            .ToArray();

        AssetDatabase.SaveAssets();

        AssetDatabase.ForceReserializeAssets(
            paths,
            ForceReserializeAssetsOptions.ReserializeAssets
        );

        Debug.Log($"重新保存完成，共处理 {paths.Length} 个资源。");
    }
}