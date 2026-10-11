using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 一键构建 Windows 版（菜单 Tools > Build Windows）。
/// 命令行用法：
///   -batchmode -quit -executeMethod BuildScript.BuildWindows -buildOut &lt;输出目录&gt;
///   可选 -onlyScene Assets/Scenes/Level_1.unity 只构建单个场景，便于单独调试关卡。
/// </summary>
public static class BuildScript
{
    static string GetArg(string name)
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == name) return args[i + 1];
        return null;
    }

    [MenuItem("Tools/Build Windows")]
    public static void BuildWindows()
    {
        string outDir = GetArg("-buildOut") ?? Path.Combine(Directory.GetCurrentDirectory(), "Build", "Windows");
        string onlyScene = GetArg("-onlyScene");

        Directory.CreateDirectory(outDir);
        string exe = Path.Combine(outDir, "Magnetic.exe");

        var scenes = new System.Collections.Generic.List<string>();
        if (!string.IsNullOrEmpty(onlyScene)) scenes.Add(onlyScene);
        else foreach (var s in EditorBuildSettings.scenes) if (s.enabled) scenes.Add(s.path);

        Debug.Log("[BuildScript] scenes: " + string.Join(", ", scenes.ToArray()));
        var opts = new BuildPlayerOptions
        {
            scenes = scenes.ToArray(),
            locationPathName = exe,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };
        BuildReport report = BuildPipeline.BuildPlayer(opts);
        Debug.Log("[BuildScript] result=" + report.summary.result + " errors=" + report.summary.totalErrors
                  + " output=" + exe);
        if (Application.isBatchMode) EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
