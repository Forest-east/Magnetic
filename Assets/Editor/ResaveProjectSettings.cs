using System;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public static class ResaveProjectSettings
{
    [MenuItem("Tools/Resave Project Settings")]
    public static void Resave()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        AssetDatabase.SaveAssets();

        string backupFolder = "ProjectSettings_Backup_" +
            DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");

        Directory.CreateDirectory(backupFolder);

        string[] paths =
            Directory.GetFiles("ProjectSettings", "*.asset");

        // 先备份全部文件，再开始处理
        foreach (string path in paths)
        {
            File.Copy(
                path,
                Path.Combine(backupFolder, Path.GetFileName(path))
            );
        }

        int saved = 0;

        foreach (string path in paths)
        {
            try
            {
                var objects =
                    InternalEditorUtility.LoadSerializedFileAndForget(path);

                if (objects == null || objects.Length == 0)
                {
                    Debug.LogWarning("未能读取，已跳过：" + path);
                    continue;
                }

                InternalEditorUtility.SaveToSerializedFileAndForget(
                    objects, path, true
                );

                saved++;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "重新保存失败：" + path + "\n" + exception.Message
                );
            }
        }

        Debug.Log(
            $"已重新保存 {saved} 个设置文件。备份目录：{backupFolder}。请重启 Unity。"
        );
    }
}