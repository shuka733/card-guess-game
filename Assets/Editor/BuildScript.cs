using UnityEditor;
using UnityEngine;
using System.IO;

public class BuildScript
{
    public static void BuildAndroid()
    {
        string buildPath = "Build/CardGuessGame.apk";
        string buildDir = Path.GetDirectoryName(buildPath);
        if (!Directory.Exists(buildDir))
            Directory.CreateDirectory(buildDir);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/GameScene.unity" },
            locationPathName = buildPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("=== APK Build Succeeded === Size: " + report.summary.totalSize + " bytes");
        }
        else
        {
            Debug.LogError("=== APK Build Failed === " + report.summary.totalErrors + " errors");
            EditorApplication.Exit(1);
        }
    }
}
