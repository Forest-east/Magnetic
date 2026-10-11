using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>验证地砖等预制体的网格引用是否已修好（检查预制体与场景两层）。</summary>
public static class MeshVerify
{
    [MenuItem("Tools/Verify Mesh References")]
    public static void Verify()
    {
        int bad = 0;

        string[] prefabs = {
            "Assets/Prefabs/Tiles/NormalTile.prefab",
            "Assets/Prefabs/Tiles/SwitchTile.prefab",
            "Assets/Prefabs/Tiles/ExitTile.prefab",
            "Assets/Prefabs/Tiles/HoleTile.prefab",
            "Assets/Prefabs/Tiles/SpawnerTile.prefab",
            "Assets/Prefabs/MetalCrate.prefab",
            "Assets/Prefabs/MetalBall.prefab",
            "Assets/Prefabs/Spikeball.prefab",
            "Assets/Prefabs/Player.prefab",
        };
        Debug.Log("[V] ===== 预制体 =====");
        foreach (var p in prefabs)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (go == null) { Debug.LogWarning("[V] " + p + " NULL"); bad++; continue; }
            int n = 0, nul = 0;
            var names = new List<string>();
            foreach (var mf in go.GetComponentsInChildren<MeshFilter>(true))
            {
                n++;
                if (mf.sharedMesh == null) { nul++; names.Add(mf.gameObject.name + "=NULL"); }
                else names.Add(mf.gameObject.name + "=" + mf.sharedMesh.name);
            }
            if (nul > 0) bad++;
            Debug.Log("[V] " + Path.GetFileName(p) + "  MeshFilter=" + n + " 空=" + nul + "  [" + string.Join(", ", names.ToArray()) + "]");
        }

        Debug.Log("[V] ===== 场景（Level_1）实际渲染物体 =====");
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Level_1.unity", OpenSceneMode.Single);
        int total = 0, nullMesh = 0, nullMat = 0;
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
            {
                total++;
                if (mf.sharedMesh == null) { nullMesh++; Debug.LogWarning("[V]   空网格: " + mf.gameObject.name); }
            }
            foreach (var mr in root.GetComponentsInChildren<MeshRenderer>(true))
                foreach (var m in mr.sharedMaterials)
                    if (m == null) nullMat++;
        }
        Debug.Log("[V] Level_1: MeshFilter=" + total + " 空网格=" + nullMesh + " 空材质=" + nullMat);
        if (nullMesh > 0) bad++;

        // 另外抽查 Level_8 和 Level_16
        foreach (var sp in new[] { "Assets/Scenes/Level_8.unity", "Assets/Scenes/Level_16.unity" })
        {
            var sc = EditorSceneManager.OpenScene(sp, OpenSceneMode.Single);
            int t = 0, nm = 0;
            foreach (var root in sc.GetRootGameObjects())
                foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
                {
                    t++;
                    if (mf.sharedMesh == null) nm++;
                }
            Debug.Log("[V] " + Path.GetFileName(sp) + ": MeshFilter=" + t + " 空网格=" + nm);
            if (nm > 0) bad++;
        }

        Debug.Log("[V] ===== 有问题的项 = " + bad + " =====");
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }
}
