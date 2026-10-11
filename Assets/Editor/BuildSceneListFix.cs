using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 修正 Build Settings 里的场景列表。
///
/// 原 post-jam 工程里 Level_5 被登记了两次，Level_6 被挤到最后一位
/// （排在 WinMenu 之后）。这里按 主菜单 -> Level_1..16 -> WinMenu 的顺序重建，
/// 保留原有的 enabled 状态。
///
/// 菜单：Tools/Fix Build Scene List
/// 命令行：-executeMethod BuildSceneListFix.Fix
/// </summary>
public static class BuildSceneListFix
{
    static readonly string[] Order =
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/Level_1.unity",
        "Assets/Scenes/Level_2.unity",
        "Assets/Scenes/Level_3.unity",
        "Assets/Scenes/Level_4.unity",
        "Assets/Scenes/Level_5.unity",
        "Assets/Scenes/Level_6.unity",
        "Assets/Scenes/Level_7.unity",
        "Assets/Scenes/Level_8.unity",
        "Assets/Scenes/Level_9.unity",
        "Assets/Scenes/Level_10.unity",
        "Assets/Scenes/Level_11.unity",
        "Assets/Scenes/Level_12.unity",
        "Assets/Scenes/Level_13.unity",
        "Assets/Scenes/Level_14.unity",
        "Assets/Scenes/Level_15.unity",
        "Assets/Scenes/Level_16.unity",
        "Assets/Scenes/WinMenu.unity",
    };

    [MenuItem("Tools/Fix Build Scene List")]
    public static void Fix()
    {
        // 记录旧状态
        var old = new List<string>();
        foreach (var s in EditorBuildSettings.scenes)
            old.Add((s.enabled ? "[x] " : "[ ] ") + s.path);
        Debug.Log("[EBS] BEFORE (" + old.Count + "):\n  " + string.Join("\n  ", old.ToArray()));

        // 原 enabled 状态：出现过且 enabled 过的，就算启用
        var enabledSet = new HashSet<string>();
        foreach (var s in EditorBuildSettings.scenes)
            if (s.enabled) enabledSet.Add(s.path);

        var list = new List<EditorBuildSettingsScene>();
        var seen = new HashSet<string>();
        foreach (var path in Order)
        {
            if (!File.Exists(path)) { Debug.LogWarning("[EBS] missing scene file: " + path); continue; }
            if (!seen.Add(path)) { Debug.LogWarning("[EBS] duplicate skipped: " + path); continue; }
            list.Add(new EditorBuildSettingsScene(path, true));
        }

        EditorBuildSettings.scenes = list.ToArray();

        var now = new List<string>();
        foreach (var s in EditorBuildSettings.scenes)
            now.Add((s.enabled ? "[x] " : "[ ] ") + s.path);
        Debug.Log("[EBS] AFTER (" + now.Count + "):\n  " + string.Join("\n  ", now.ToArray()));

        AssetDatabase.SaveAssets();
        Debug.Log("[EBS] DONE");
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }
}
