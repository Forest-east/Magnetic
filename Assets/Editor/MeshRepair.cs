using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 修复预制体里因为 .blend -> .fbx 转换而失效的网格与材质引用。
///
/// 原因：Blender 导出的 FBX 里子网格被重命名（tile -> Cube.000 等），
/// 原预制体按旧名字引用，转换后全部变成 null，物体就不再渲染。
///
/// 菜单：Tools/Repair Mesh References
/// 命令行：-executeMethod MeshRepair.RepairAll
/// </summary>
public static class MeshRepair
{
    // 预制体 -> 提供网格的 FBX
    static readonly Dictionary<string, string> PrefabToFbx = new Dictionary<string, string>
    {
        { "Assets/Prefabs/Tiles/NormalTile.prefab",  "Assets/Models/tile.fbx" },
        { "Assets/Prefabs/Tiles/SwitchTile.prefab",  "Assets/Models/button.fbx" },
        { "Assets/Prefabs/Tiles/ExitTile.prefab",    "Assets/Models/portal.fbx" },
        { "Assets/Prefabs/Tiles/HoleTile.prefab",    "Assets/Models/hole.fbx" },
        { "Assets/Prefabs/Tiles/SpawnerTile.prefab", "Assets/Models/spawner.fbx" },
        { "Assets/Prefabs/MetalCrate.prefab",        "Assets/Models/crate.fbx" },
        { "Assets/Prefabs/MetalBall.prefab",         "Assets/Models/spikeball.fbx" },
        { "Assets/Prefabs/Spikeball.prefab",         "Assets/Models/spikeball.fbx" },
        { "Assets/Prefabs/Player.prefab",            "Assets/Models/player.fbx" },
    };

    [MenuItem("Tools/Repair Mesh References")]
    public static void RepairAll()
    {
        int totalFixed = 0, filesTouched = 0;

        foreach (var kv in PrefabToFbx)
        {
            string prefabPath = kv.Key, fbxPath = kv.Value;
            if (!File.Exists(prefabPath)) { Debug.LogWarning("[MR] missing prefab " + prefabPath); continue; }
            if (!File.Exists(fbxPath)) { Debug.LogWarning("[MR] missing fbx " + fbxPath); continue; }

            // 收集该 FBX 的所有网格，按名字排序
            var meshes = new List<Mesh>();
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(fbxPath))
            {
                var m = o as Mesh;
                if (m != null) meshes.Add(m);
            }
            if (meshes.Count == 0) { Debug.LogWarning("[MR] no meshes in " + fbxPath); continue; }
            meshes.Sort((a, b) => string.CompareOrdinal(a.name, b.name));

            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            if (root == null) { Debug.LogWarning("[MR] cannot load " + prefabPath); continue; }

            var filters = root.GetComponentsInChildren<MeshFilter>(true);
            var broken = new List<MeshFilter>();
            foreach (var f in filters) if (f.sharedMesh == null) broken.Add(f);

            int fixedHere = 0;
            if (broken.Count > 0)
            {
                // 按子物体顺序对应网格顺序
                for (int i = 0; i < broken.Count; i++)
                {
                    var mesh = meshes[i < meshes.Count ? i : meshes.Count - 1];
                    string old = broken[i].sharedMesh == null ? "NULL" : broken[i].sharedMesh.name;
                    broken[i].sharedMesh = mesh;
                    fixedHere++;
                    Debug.Log("[MR]   " + Path.GetFileName(prefabPath) + " :: '"
                              + broken[i].gameObject.name + "'  " + old + " -> " + mesh.name);
                }
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                filesTouched++;
                totalFixed += fixedHere;
            }
            else
            {
                Debug.Log("[MR]   " + Path.GetFileName(prefabPath) + " : 无空网格（" + filters.Length + " 个 MeshFilter）");
            }
            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[MR] DONE fixed=" + totalFixed + " prefabsTouched=" + filesTouched);
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }
}
