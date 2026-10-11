using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 为每个场景重建光照数据。
/// 原工程用 Unity 5.5 烘焙的 LightingData 在 Unity 6 下不兼容，会报
///   "Lighting data asset 'LightingData' is incompatible with the current Unity version"
/// 这里重新烘焙一遍即可。
///
/// 菜单：Tools/Rebuild Lighting For All Scenes
/// 命令行：-executeMethod LightingFix.RebuildAll
/// </summary>
public static class LightingFix
{
    static readonly string[] Scenes =
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/Level_1.unity",
        "Assets/Scenes/Level_2.unity",
        "Assets/Scenes/Level_3.unity",
        "Assets/Scenes/Level_4.unity",
        "Assets/Scenes/Level_5.unity",
        "Assets/Scenes/Level_6.unity",
        "Assets/Scenes/WinMenu.unity",
        "Assets/Scenes/Sanbox.unity",
    };

    [MenuItem("Tools/Rebuild Lighting For All Scenes")]
    public static void RebuildAll()
    {
        int ok = 0, fail = 0;
        foreach (var path in Scenes)
        {
            if (!File.Exists(path)) { Debug.LogWarning("[Light] missing " + path); continue; }
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            Debug.Log("[Light] baking " + path + " ...");
            bool baked = Lightmapping.Bake();
            Debug.Log("[Light] " + path + " -> " + (baked ? "SUCCESS" : "FAILED"));
            if (baked) ok++; else fail++;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Light] DONE ok=" + ok + " fail=" + fail);
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }
}
