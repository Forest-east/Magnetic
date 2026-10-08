using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 项目健康检查：扫描所有预制体与场景，报告
///   - 空网格 / 空材质槽 / 着色器损坏
///   - 丢失脚本组件
///
/// 菜单：Tools/Project Health Check
/// 命令行：-executeMethod ProjectHealth.Check
/// </summary>
public static class ProjectHealth
{
    static int problems;

    static void P(string msg)
    {
        problems++;
        Debug.LogWarning("[HEALTH] " + msg);
    }

    [MenuItem("Tools/Project Health Check")]
    public static void Check()
    {
        problems = 0;

        foreach (var g in AssetDatabase.FindAssets("t:Prefab"))
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) { P("prefab cannot load: " + path); continue; }
            Scan(go, path);
        }

        foreach (var g in AssetDatabase.FindAssets("t:SceneAsset"))
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            foreach (var root in scene.GetRootGameObjects())
                Scan(root, path);
        }

        foreach (var g in AssetDatabase.FindAssets("t:Material"))
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { P("material cannot load: " + path); continue; }
            if (m.shader == null) P("material has NO shader: " + path);
            else if (m.shader.name == "Hidden/InternalErrorShader")
                P("material uses InternalErrorShader: " + path);
        }

        Debug.Log("[HEALTH] ===== TOTAL PROBLEMS = " + problems + " =====");
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }

    static void Scan(GameObject root, string context)
    {
        foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
            if (mf.sharedMesh == null) P(context + " :: '" + mf.gameObject.name + "' MeshFilter mesh NULL");

        foreach (var smr in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            if (smr.sharedMesh == null) P(context + " :: '" + smr.gameObject.name + "' SkinnedMeshRenderer mesh NULL");

        foreach (var mr in root.GetComponentsInChildren<MeshRenderer>(true))
        {
            var mats = mr.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) P(context + " :: '" + mr.gameObject.name + "' material slot " + i + " NULL");
                else if (mats[i].shader == null || mats[i].shader.name == "Hidden/InternalErrorShader")
                    P(context + " :: '" + mr.gameObject.name + "' material '" + mats[i].name + "' broken shader");
            }
        }

        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
            if (missing > 0) P(context + " :: '" + t.gameObject.name + "' has " + missing + " MISSING SCRIPT(s)");
        }
    }
}
