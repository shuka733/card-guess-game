using UnityEditor;
using UnityEngine;
using System.IO;

public class BuildScript
{
    /// <summary>
    /// CI/CD から呼び出されるエントリポイント。
    /// シーンを自動生成してから Android APK をビルドする。
    /// </summary>
    public static void BuildAndroid()
    {
        // 1. シーン構築（CI では必ず実行）
        Debug.Log("[BuildScript] Building horror scene...");
        HorrorSceneBuilder.BuildScene();

        // 2. APK ビルド
        string buildPath = "Build/SilentWard.apk";
        string buildDir  = Path.GetDirectoryName(buildPath);
        if (!Directory.Exists(buildDir))
            Directory.CreateDirectory(buildDir);

        var options = new BuildPlayerOptions
        {
            scenes           = new[] { "Assets/Scenes/HorrorGame.unity" },
            locationPathName = buildPath,
            target           = BuildTarget.Android,
            options          = BuildOptions.None,
        };

        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("[BuildScript] APK build succeeded. Size: " + report.summary.totalSize + " bytes");
        }
        else
        {
            Debug.LogError("[BuildScript] APK build FAILED. Errors: " + report.summary.totalErrors);
            EditorApplication.Exit(1);
        }
    }
}
