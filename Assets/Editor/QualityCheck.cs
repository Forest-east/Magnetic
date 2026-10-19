using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>检查画质设置里的抗锯齿与阴影等，判断刺球"闪烁"是否因缺少抗锯齿。</summary>
public static class QualityCheck
{
    [MenuItem("Tools/Check Quality Settings")]
    public static void Check()
    {
        Debug.Log("[Q] 当前画质等级 = " + QualitySettings.GetQualityLevel() + " / " + QualitySettings.names.Length);
        Debug.Log("[Q] antiAliasing = " + QualitySettings.antiAliasing);
        Debug.Log("[Q] shadowDistance = " + QualitySettings.shadowDistance);
        Debug.Log("[Q] shadowCascades = " + QualitySettings.shadowCascades);
        Debug.Log("[Q] shadowResolution = " + QualitySettings.shadowResolution);
        Debug.Log("[Q] softParticles = " + QualitySettings.softParticles);
        Debug.Log("[Q] 各画质等级的 antiAliasing：");
        int cur = QualitySettings.GetQualityLevel();
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            Debug.Log("[Q]   [" + i + "] " + QualitySettings.names[i] + "  antiAliasing=" + QualitySettings.antiAliasing
                      + "  shadowDistance=" + QualitySettings.shadowDistance);
        }
        QualitySettings.SetQualityLevel(cur, false);

        // 场景里所有 MeshRenderer 的三角形总数 Top 10
        Debug.Log("[Q] ===== 场景中三角形最多的物体 =====");
        var counts = new System.Collections.Generic.List<(string, int, int)>();
        foreach (var mr in Object.FindObjectsOfType<MeshRenderer>(true))
        {
            var mf = mr.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;
            var tris = mf.sharedMesh.triangles.Length / 3;
            counts.Add((mr.gameObject.name, tris, mf.sharedMesh.vertexCount));
        }
        counts.Sort((a, b) => b.Item2.CompareTo(a.Item2));
        for (int i = 0; i < Mathf.Min(10, counts.Count); i++)
            Debug.Log("[Q]   " + counts[i].Item1 + "  tris=" + counts[i].Item2 + "  verts=" + counts[i].Item3);
        int total = 0;
        foreach (var c in counts) total += c.Item2;
        Debug.Log("[Q] 场景三角形总计 = " + total + " （物体数 " + counts.Count + "）");

        if (Application.isBatchMode) EditorApplication.Exit(0);
    }
}
