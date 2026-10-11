using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 列出预制体/场景里"丢失脚本"槽位所引用的脚本 GUID，
/// 这样就能确定每个空槽原本挂的是哪个 .cs。
/// 菜单：Tools/List Missing Script GUIDs
/// </summary>
public static class ProjectHealth
{
    static readonly string[] Prefabs =
    {
        "Assets/Prefabs/_CameraRig.prefab",
        "Assets/Prefabs/GameManager.prefab",
        "Assets/Prefabs/LevelController.prefab",
        "Assets/Prefabs/LevelMenuCanvas.prefab",
        "Assets/Prefabs/LevelSelectButton.prefab",
        "Assets/Prefabs/DeathPlane.prefab",
        "Assets/Prefabs/ExplosionEffect.prefab",
        "Assets/Prefabs/MenuButton.prefab",
        "Assets/Prefabs/MenuMusic.prefab",
        "Assets/Prefabs/LevelMusic.prefab",
        "Assets/Prefabs/BackgroundCamera.prefab",
        "Assets/Prefabs/MetalCrate.prefab",
        "Assets/Prefabs/Player.prefab",
        "Assets/Prefabs/Spikeball.prefab",
        "Assets/Prefabs/Tiles/NormalTile.prefab",
        "Assets/Prefabs/Tiles/HoleTile.prefab",
        "Assets/Prefabs/Tiles/SwitchTile.prefab",
        "Assets/Prefabs/Tiles/ExitTile.prefab",
        "Assets/Prefabs/Tiles/SpawnerTile.prefab",
    };

    [MenuItem("Tools/List Missing Script GUIDs")]
    public static void Check()
    {
        // 建立 GUID -> 脚本名 索引
        var byGuid = new Dictionary<string, string>();
        foreach (var g in AssetDatabase.FindAssets("t:MonoScript"))
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            var ms = AssetDatabase.LoadAssetAtPath<MonoScript>(p);
            string name = ms != null ? ms.GetClass().FullName : Path.GetFileNameWithoutExtension(p);
            byGuid[g] = name + "   (" + p + ")";
        }

        foreach (var path in Prefabs)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) { Debug.LogWarning("[G] cannot load " + path); continue; }
            var lines = new List<string>();
            foreach (var t in go.GetComponentsInChildren<Transform>(true))
            {
                var so = new SerializedObject(t.gameObject);
                var arr = so.FindProperty("m_Component");
                if (arr == null) continue;
                for (int i = 0; i < arr.arraySize; i++)
                {
                    var e = arr.GetArrayElementAtIndex(i);
                    var pptr = e.FindPropertyRelative("component");
                    if (pptr == null) continue;
                    var fileID = pptr.FindPropertyRelative("m_FileID").intValue;
                    var pathID = pptr.FindPropertyRelative("m_PathID").longValue;
                    if (pathID != 0 || fileID != 0) continue;

                    lines.Add("      MISSING slot " + i + " on '" + t.gameObject.name + "'");
                }
            }
            if (lines.Count > 0)
            {
                Debug.Log("[G] " + Path.GetFileName(path) + "  (" + lines.Count + " missing)");
                foreach (var l in lines) Debug.Log("[G] " + l);
            }
        }

        if (Application.isBatchMode) EditorApplication.Exit(0);
    }
}
